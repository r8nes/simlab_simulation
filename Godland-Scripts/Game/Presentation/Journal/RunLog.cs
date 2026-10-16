using System;
using System.IO;
using System.Linq;
using Godland.Core.Fate.Cataclysms;
using Godland.Core.World.Creation;
using Godland.Core.Fate.Epochs;
using Godland.Core.World.Grid;
using Godland.Core.Life.People;
using Godland.Core.Randomness;
using Godland.Core.World.Tiles;
using Godland.Core.World.Timing;
using Godland.Game.Writer.Actions;
using Godland.Game.World.Evolution;
using Godland.Game.Life.Features;
using Godland.Game.Writer.Gifts;
using Godland.Game.Life.People;
using Godland.Game.World.Tiles;
using UnityEngine;
using VContainer.Unity;

namespace Godland.Game.Presentation.Journal
{
    public sealed class RunLog : IStartable, IDisposable
    {
        private const int SnapshotPeriod = 50;

        private readonly WorldClock _clock;
        private readonly SeededRandom _random;
        private readonly EventJournal _journal;
        private readonly TilePlacement _placement;
        private readonly CellPurchase _purchase;
        private readonly TileActions _actions;
        private readonly TileUpgrades _upgrades;
        private readonly TileEvolutions _evolutions;
        private readonly GiftOffers _gifts;
        private readonly WorldTiles _tiles;
        private readonly TileCatalog _catalog;
        private readonly CreationPoints _creation;
        private readonly FaithPoints _faith;
        private readonly WorldEpoch _epoch;
        private readonly Settlements _settlements;
        private readonly Shrines _shrines;
        private readonly IdolCataclysms _cataclysms;
        private readonly SettlementConfig _settlementConfig;
        private readonly Godland.Game.Writer.Commandments.CommandmentKeeper _commandments;
        private readonly System.Collections.Generic.List<Godland.Game.Presentation.Overview.IWorldReport> _reports;
        private const int ReportPeriod = 100;
        private StreamWriter _writer;

        private readonly Godland.Game.Writer.Commands.EntityCommands _commands;

        public RunLog(
            WorldClock clock, SeededRandom random, EventJournal journal, TilePlacement placement, CellPurchase purchase,
            TileActions actions, TileUpgrades upgrades, TileEvolutions evolutions, GiftOffers gifts,
            WorldTiles tiles, TileCatalog catalog, CreationPoints creation, FaithPoints faith, WorldEpoch epoch,
            Settlements settlements, Shrines shrines, IdolCataclysms cataclysms, SettlementConfig settlementConfig,
            Godland.Game.Writer.Commandments.CommandmentKeeper commandments, System.Collections.Generic.IEnumerable<Godland.Game.Presentation.Overview.IWorldReport> reports,
            Godland.Game.Writer.Commands.EntityCommands commands)
        {
            _commands = commands;
            _reports = reports.ToList();
            _commandments = commandments;
            _clock = clock;
            _random = random;
            _journal = journal;
            _placement = placement;
            _purchase = purchase;
            _actions = actions;
            _upgrades = upgrades;
            _evolutions = evolutions;
            _gifts = gifts;
            _tiles = tiles;
            _catalog = catalog;
            _creation = creation;
            _faith = faith;
            _epoch = epoch;
            _settlements = settlements;
            _shrines = shrines;
            _cataclysms = cataclysms;
            _settlementConfig = settlementConfig;
        }

        public void Start()
        {
            var folder = Path.Combine(Application.dataPath, "..", "Logs", "Runs");
            Directory.CreateDirectory(folder);
            _writer = new StreamWriter(Path.Combine(folder, $"run-{DateTime.Now:yyyyMMdd-HHmmss}.log")) { AutoFlush = true };
            _writer.WriteLine($"# Godland run {DateTime.Now:yyyy-MM-dd HH:mm:ss}, seed {_random.Seed}");

            _commandments.Proclaimed += OnProclaimed;
            _journal.Changed += OnJournal;
            _placement.Placed += OnPlaced;
            _purchase.Purchased += OnPurchased;
            _actions.Applied += OnAction;
            _commands.Issued += OnCommand;
            _upgrades.Applied += OnUpgrade;
            _evolutions.Evolved += OnEvolved;
            _gifts.Chosen += OnGift;
            _clock.Ticked += OnTicked;
        }

        public void Dispose()
        {
            _commandments.Proclaimed -= OnProclaimed;
            _journal.Changed -= OnJournal;
            _placement.Placed -= OnPlaced;
            _purchase.Purchased -= OnPurchased;
            _actions.Applied -= OnAction;
            _commands.Issued -= OnCommand;
            _upgrades.Applied -= OnUpgrade;
            _evolutions.Evolved -= OnEvolved;
            _gifts.Chosen -= OnGift;
            _clock.Ticked -= OnTicked;
            _writer?.Dispose();
        }

        private void WriteReports()
        {
            foreach (var report in _reports)
            foreach (var line in report.Lines())
            {
                var text = System.Text.RegularExpressions.Regex.Replace(line.Text ?? string.Empty, "<.*?>", string.Empty).Replace("\n", " | ");
                var place = line.Place is { } cell ? $" ({cell.X}, {cell.Y})" : string.Empty;
                Write("REPORT", $"[{report.Title}] {line.Title.Trim()}{place}: {text}");
            }
        }

        private void OnProclaimed(Godland.Game.Writer.Commandments.Commandment commandment) => Write("RULE", $"Заповедь: {commandment.Text}");

        private void Write(string kind, string text) => _writer?.WriteLine($"{_clock.Year,5} {kind,-7} {text}");

        private string TileAt(GridCoord coord) => _tiles.TryGet(coord, out var id) ? _catalog.Get(id).DisplayName : "пусто";

        private void OnJournal()
        {
            var entry = _journal.Entries[^1];
            if (entry.Repeats == 1) Write("WORLD", entry.Text);
        }

        private void OnPlaced(GridCoord coord, TileDefinition tile) => Write("PLAYER", $"поставил {tile.DisplayName} {coord}");

        private void OnPurchased(GridCoord coord) => Write("PLAYER", $"купил клетку {coord}, следующая {_purchase.Cost}");

        private void OnCommand(Godland.Game.Writer.Commands.EntityCommand command, Godland.Core.World.Grid.GridCoord? place) => Write("PLAYER", $"приказ: {command.Title} ({command.Entity}) {place}");

        private void OnAction(GridCoord coord, TileActionDefinition action) => Write("PLAYER", $"{action.DisplayName} на {TileAt(coord)} {coord}");

        private void OnUpgrade(GridCoord coord, FeatureDefinition upgrade) => Write("PLAYER", $"подсадил {upgrade.DisplayName} на {TileAt(coord)} {coord}");

        private void OnEvolved(GridCoord coord, TileDefinition tile) => Write("PLAYER", $"эволюция в {tile.DisplayName} {coord}");

        private void OnGift(GiftDefinition gift) => Write("PLAYER", $"выбрал дар {gift.name}");

        private void OnTicked(int year)
        {
            if (year % ReportPeriod == 0) WriteReports();
            if (year % SnapshotPeriod != 0) return;

            var population = _settlements.All.Values.Sum();
            Write("STATE", $"эпоха {_epoch.Index}, созидание {_creation.Amount}, вера {_faith.Amount}, тайлов {_tiles.CountedCount}, " +
                           $"поселений {_settlements.All.Count}, жителей {population}, верующих {_shrines.BelieverCount}");

            foreach (var religion in _shrines.Religions)
            {
                var god = religion.IdolId != null ? _settlementConfig.IdolById(religion.IdolId).DisplayName
                    : religion.Shrine is { } shrine ? TileAt(shrine) : "?";
                var heresy = religion.IsHeresy ? " (ересь)" : string.Empty;
                Write("STATE", $"  вера {god}{heresy}: {_shrines.FollowersOf(religion).Count} пос., {_cataclysms.SharePercent(religion)}%, пыл {religion.Fervor}, догмат {religion.Dogma.Id}");
            }

            if (_cataclysms.Current is { } summoning)
                Write("STATE", $"  призыв идёт: {summoning.YearsLeft} лет");
        }
    }
}
