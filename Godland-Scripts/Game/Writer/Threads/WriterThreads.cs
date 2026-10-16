using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core;
using Godland.Core.World.Cells;
using Godland.Core.World.Grid;
using Godland.Core.Life.People;
using Godland.Game.Writer.Actions;
using Godland.Game.Writer.Commands;
using Godland.Game.Fate.Emergence;
using Godland.Game.World.Evolution;
using Godland.Game.Life.Features;
using Godland.Game.World.Tiles;

namespace Godland.Game.Writer.Threads
{
    public enum WriterTouch
    {
        Create,
        Command,
        Miracle
    }

    public enum WriterPath
    {
        None,
        Creator,
        Director,
        Observer
    }

    public sealed class Prophet
    {
        public Prophet(string name, GridCoord home, int born)
        {
            Name = name;
            Home = home;
            Born = born;
        }

        public string Name { get; }
        public GridCoord Home { get; }
        public int Born { get; }
        public int Truths { get; set; }
        public int Lies { get; set; }
    }

    public sealed class WriterThreads : IWorldStep, IDisposable
    {
        private const string Favored = "favored";
        private const string BlessedSign = "blessed-sign";
        private const string WrathSign = "wrath-sign";

        private readonly WriterConfig _config;
        private readonly EmergenceContext _world;
        private readonly Godfighters _godfighters;
        private readonly CellFertility _fertility;
        private readonly CommandDiscount _discount;
        private readonly EntityCommands _commands;
        private readonly TileActions _actions;
        private readonly TilePlacement _placement;
        private readonly TileEvolutions _evolutions;
        private readonly TileUpgrades _upgrades;
        private readonly Dictionary<WriterPath, int> _pathScore = new();
        private readonly Dictionary<WriterTouch, int> _century = new();
        private int _nameIndex;
        private int _silentSince = int.MinValue / 2;

        public WriterThreads(
            WriterConfig config, EmergenceContext world, Godfighters godfighters, CellFertility fertility, CommandDiscount discount,
            EntityCommands commands, TileActions actions, TilePlacement placement, TileEvolutions evolutions, TileUpgrades upgrades)
        {
            _config = config;
            _world = world;
            _godfighters = godfighters;
            _fertility = fertility;
            _discount = discount;
            _commands = commands;
            _actions = actions;
            _placement = placement;
            _evolutions = evolutions;
            _upgrades = upgrades;

            _commands.Issued += OnIssued;
            _actions.Applied += OnAction;
            _placement.Placed += OnPlaced;
            _evolutions.Evolved += OnPlaced;
            _upgrades.Applied += OnUpgraded;
        }

        public event Action<string, GridCoord?> Happened;

        public Prophet Prophet { get; private set; }
        public WriterPath Path { get; private set; }
        public IReadOnlyDictionary<WriterPath, int> PathScore => _pathScore;

        public void Dispose()
        {
            _commands.Issued -= OnIssued;
            _actions.Applied -= OnAction;
            _placement.Placed -= OnPlaced;
            _evolutions.Evolved -= OnPlaced;
            _upgrades.Applied -= OnUpgraded;
        }

        public void Touch(GridCoord site, WriterTouch touch)
        {
            _century[touch] = _century.GetValueOrDefault(touch) + 1;
            _world.Memory.Add(site, Favored);
            if (touch != WriterTouch.Create) Interpret(site);
        }

        public void Advance(int year)
        {
            if (year % _config.SignPeriod == 0) ResolveSigns();
            if (year % _config.DebtPeriod == 0) CollectDebts();
            UpdateProphet(year);
            if (_config.PathsEnabled) UpdatePath(year);
        }

        private void OnIssued(EntityCommand command, GridCoord? place)
        {
            if (place is { } site) Touch(site, WriterTouch.Command);
        }

        private void OnAction(GridCoord cell, TileActionDefinition action)
        {
            Touch(cell, action.FaithCost > 0 ? WriterTouch.Miracle : WriterTouch.Create);
            if (action.FaithCost == 0) Interpret(cell);
        }

        private void OnPlaced(GridCoord cell, TileDefinition _) => Touch(cell, WriterTouch.Create);

        private void OnUpgraded(GridCoord cell, FeatureDefinition _) => Touch(cell, WriterTouch.Create);

        private IEnumerable<GridCoord> Around(GridCoord site, int radius) =>
            _world.Settlements.All.Keys
                .Where(cell => Math.Abs(cell.X - site.X) + Math.Abs(cell.Y - site.Y) <= radius)
                .OrderBy(cell => cell.X).ThenBy(cell => cell.Y)
                .ToList();

        private Religion FaithAt(GridCoord site) =>
            Around(site, _config.WitnessRadius)
                .OrderBy(cell => Math.Abs(cell.X - site.X) + Math.Abs(cell.Y - site.Y))
                .Select(cell => _world.Shrines.TryGet(cell, out var faith) ? faith : null)
                .FirstOrDefault();

        private void Interpret(GridCoord site)
        {
            var faith = FaithAt(site);
            foreach (var witness in Around(site, _config.WitnessRadius))
            {
                _world.Shrines.TryGet(witness, out var own);
                _world.Memory.Add(witness, own == faith ? BlessedSign : WrathSign);
            }
        }

        private void ResolveSigns()
        {
            foreach (var cell in _world.Settlements.All.Keys.OrderBy(cell => cell.X).ThenBy(cell => cell.Y).ToList())
            {
                if (_world.Memory.Get(cell, BlessedSign) >= _config.SignThreshold)
                {
                    _world.Memory.Forget(cell, BlessedSign, 0);
                    _world.Settlements.Bless(cell, 1);
                    if (_world.Shrines.TryGet(cell, out var faith)) faith.Kindle(10);
                    _world.Memory.Add(cell, "grace");
                    Happened?.Invoke("Знак милости: люди сочли вмешательство писателя благословением своей веры", cell);
                }

                if (_world.Settlements.All.ContainsKey(cell) && _world.Memory.Get(cell, WrathSign) >= _config.SignThreshold)
                {
                    _world.Memory.Forget(cell, WrathSign, 0);
                    _world.Settlements.Weaken(cell, 1);
                    _world.Memory.Add(cell, "war");
                    _world.Memory.Add(cell, "sky-fear");
                    Happened?.Invoke("Страх перед небом: чужая вера сочла вмешательство писателя гневом богов", cell);
                }
            }
        }

        private void CollectDebts()
        {
            var debtor = _world.Settlements.All.Keys
                .Where(cell => _world.Memory.Around(cell, Favored, 1) >= _config.DebtThreshold)
                .OrderByDescending(cell => _world.Memory.Around(cell, Favored, 1)).ThenBy(cell => cell.X).ThenBy(cell => cell.Y)
                .Cast<GridCoord?>().FirstOrDefault();
            if (debtor is not { } cell) return;

            _world.Memory.Forget(cell, Favored, 1);
            if (_world.Random.Next(100) < _config.TyrantPercent)
            {
                foreach (var neighbour in Around(cell, 1).Where(other => other != cell)) _world.Settlements.Weaken(neighbour, 1);
                _world.Settlements.Bless(cell, 2);
                _world.Memory.Add(cell, "tyrant");
                Happened?.Invoke("Избранники писателя возгордились: любимое место стало тираном соседей", cell);
                return;
            }

            _godfighters.Witness(cell);
            _godfighters.Witness(cell);
            _world.Memory.Add(cell, "anti-writer-cult");
            Happened?.Invoke("Долг невидимых нитей: в любимом месте писателя родился культ против него", cell);
        }

        private void UpdateProphet(int year)
        {
            if (Prophet == null)
            {
                if (year % _config.ProphetPeriod != 0 || year - _silentSince < _config.ProphetGapYears) return;

                var home = _world.Settlements.All.Keys
                    .Where(cell => _world.Memory.Around(cell, Favored, 1) >= _config.ProphetThreshold)
                    .OrderByDescending(cell => _world.Memory.Around(cell, Favored, 1)).ThenBy(cell => cell.X).ThenBy(cell => cell.Y)
                    .Cast<GridCoord?>().FirstOrDefault();
                if (home is not { } cell || _config.ProphetNames.Count == 0) return;

                Prophet = new Prophet(_config.ProphetNames[_nameIndex++ % _config.ProphetNames.Count], cell, year);
                _discount.Set("prophet", _config.ProphetDiscountPercent);
                _world.Memory.Add(cell, "prophet");
                Happened?.Invoke($"Родился пророк {Prophet.Name}: он говорит от имени писателя", cell);
                return;
            }

            if (!_world.Settlements.All.ContainsKey(Prophet.Home) || year - Prophet.Born >= _config.ProphetLifeYears)
            {
                Happened?.Invoke($"Пророк {Prophet.Name} умолк навсегда (правда: {Prophet.Truths}, ложь: {Prophet.Lies})", Prophet.Home);
                Prophet = null;
                _silentSince = year;
                _discount.Set("prophet", 0);
                return;
            }

            if ((year - Prophet.Born) % _config.ProphetPeriod != 0) return;

            if (_world.Random.Next(100) < _config.ProphetLiePercent)
            {
                Prophet.Lies++;
                foreach (var cell in Around(Prophet.Home, 1).Where(cell => cell != Prophet.Home)) _world.Settlements.Weaken(cell, 1);
                _world.Memory.Add(Prophet.Home, "false-prophecy");
                Happened?.Invoke($"Пророк {Prophet.Name} исказил волю писателя: соседи пострадали от ложного пророчества", Prophet.Home);
                return;
            }

            Prophet.Truths++;
            if (_world.Shrines.TryGet(Prophet.Home, out var faith)) faith.Kindle(10);
            foreach (var cell in Around(Prophet.Home, 1)) _world.Settlements.Bless(cell, 1);
            _world.Memory.Add(Prophet.Home, "true-prophecy");
            Happened?.Invoke($"Пророк {Prophet.Name} верно передал волю писателя", Prophet.Home);
        }

        private void UpdatePath(int year)
        {
            if (Path == WriterPath.Creator && year % _config.CreatorPulsePeriod == 0) Pulse();
            if (year % _config.PathPeriod != 0 || year == 0) return;

            var creates = _century.GetValueOrDefault(WriterTouch.Create);
            var directs = _century.GetValueOrDefault(WriterTouch.Command) + _century.GetValueOrDefault(WriterTouch.Miracle);
            var lead = creates + directs <= _config.ObserverMaxTouches ? WriterPath.Observer : directs * _config.DirectorWeight >= creates ? WriterPath.Director : WriterPath.Creator;
            _century.Clear();
            _pathScore[lead] = _pathScore.GetValueOrDefault(lead) + 1;

            var path = _pathScore.OrderByDescending(pair => pair.Value).ThenBy(pair => pair.Key == Path ? 0 : 1).First().Key;
            if (path == Path) return;

            Leave(Path);
            Path = path;
            Enter(Path);
            Happened?.Invoke($"Путь писателя: {NameOf(Path)}", null);
        }

        private void Enter(WriterPath path)
        {
            if (path == WriterPath.Director) _discount.Set("path", _config.DirectorDiscountPercent);
            if (path == WriterPath.Observer && _world.Modifiers.EmergenceChancePercent == 100) _world.Modifiers.SetEmergenceChance(_config.ObserverEmergencePercent);
        }

        private void Leave(WriterPath path)
        {
            if (path == WriterPath.Director) _discount.Set("path", 0);
            if (path == WriterPath.Observer && _world.Modifiers.EmergenceChancePercent == _config.ObserverEmergencePercent) _world.Modifiers.SetEmergenceChance(100);
        }

        private void Pulse()
        {
            var cells = _world.Tiles.All.Keys.OrderBy(cell => cell.X).ThenBy(cell => cell.Y).ToList();
            for (var i = 0; i < _config.CreatorPulseCells && cells.Count > 0; i++)
            {
                var index = _world.Random.Next(cells.Count);
                _fertility.Add(cells[index], 1);
                cells.RemoveAt(index);
            }
        }

        public static string NameOf(WriterPath path) => path switch
        {
            WriterPath.Creator => "Творец — мир плодороднее",
            WriterPath.Director => "Режиссёр — приказы дешевле",
            WriterPath.Observer => "Наблюдатель — мир чаще рождает новое",
            _ => "не выбран"
        };
    }
}
