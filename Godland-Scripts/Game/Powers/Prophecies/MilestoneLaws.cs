using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Grid;
using Godland.Game.Fate.Catastrophes;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Powers.Prophecies
{
    public enum WorldRule
    {
        BarrenLand,
        NoHunting,
        Godless,
        EternalWar,
        NoWars,
        NoCities
    }

    [Serializable]
    public sealed class WorldRuleLaw : IAgeLaw
    {
        [SerializeField] private WorldRule _rule;

        public void Begin(CatastropheContext context, string cause)
        {
            var world = context.World;
            switch (_rule)
            {
                case WorldRule.BarrenLand: world.Settlements.MakeLandBarren(); break;
                case WorldRule.NoHunting: world.Settlements.ForbidHunting(); break;
                case WorldRule.Godless: world.Settlements.MakeGodless(); break;
                case WorldRule.EternalWar: world.Realms.MakeWarsEternal(); break;
                case WorldRule.NoWars: world.Realms.ForbidWars(); break;
                case WorldRule.NoCities: context.Cities.ForbidCities(); break;
            }
        }

        public void Year(CatastropheContext context, int age, string cause) { }

        public void End(CatastropheContext context) { }
    }

    [Serializable]
    public sealed class FaithLaw : IAgeLaw
    {
        [SerializeField, MinValue(1), SuffixLabel("%")] private int _faithPercent = 200;
        [SerializeField, MinValue(1), SuffixLabel("years")] private int _period = 20;
        [SerializeField, MinValue(0), InfoBox("Сколько рвения получает каждая вера за период.")] private int _fervor;

        public void Begin(CatastropheContext context, string cause) => context.World.Settlements.BoostFaith(_faithPercent);

        public void Year(CatastropheContext context, int age, string cause)
        {
            if (_fervor == 0 || age % _period != 0) return;

            foreach (var religion in context.World.Shrines.Religions) religion.Kindle(_fervor);
        }

        public void End(CatastropheContext context) { }
    }

    [Serializable]
    public sealed class ColonyLaw : IAgeLaw
    {
        [SerializeField, MinValue(1), SuffixLabel("years")] private int _period = 25;
        [SerializeField, InfoBox("Отделяются только поселения, достигшие вместимости.")] private bool _atCapacity = true;
        [SerializeField, MinValue(2)] private int _minPopulation = 4;
        [SerializeField, InfoBox("Уходит половина жителей (кочевники), иначе новое поселение рождается сверх старого.")] private bool _takeHalf;
        [SerializeField, MinValue(1)] private int _colonists = 2;

        public void Begin(CatastropheContext context, string cause) { }

        public void Year(CatastropheContext context, int age, string cause)
        {
            if (age % _period != 0) return;

            var world = context.World;
            foreach (var (cell, population) in world.Settlements.All.ToList())
            {
                if (population < _minPopulation || (_atCapacity && population < world.Settlements.CapacityOf(cell))) continue;

                var colonists = _takeHalf ? population / 2 : _colonists;
                var target = context.Shuffled(world.Grid.Neighbors(cell)).FirstOrDefault(neighbor => world.Settlements.TrySettle(neighbor, colonists));
                if (target == default) continue;

                if (_takeHalf) world.Settlements.Weaken(cell, colonists);
                world.History.Note(target, $"Сюда пришли люди из соседнего поселения ({cause})");
            }
        }

        public void End(CatastropheContext context) { }
    }

    [Serializable]
    public sealed class BeastLaw : IAgeLaw
    {
        [SerializeField, MinValue(1), SuffixLabel("years")] private int _period = 30;
        [SerializeField, MinValue(1), InfoBox("Стая из стольких соседних зверей нападает на поселение; меньше — заключает союз.")] private int _pack = 2;
        [SerializeField, MinValue(0), InfoBox("Союз со зверьми: столько жителей сверх вместимости.")] private int _allyBonus = 1;

        public void Begin(CatastropheContext context, string cause) { }

        public void Year(CatastropheContext context, int age, string cause)
        {
            if (age % _period != 0) return;

            var world = context.World;
            foreach (var (cell, population) in world.Settlements.All.ToList())
            {
                var beasts = world.Grid.Neighbors(cell).Count(neighbor => world.Features.TryGet(neighbor, out _));
                if (beasts >= _pack)
                {
                    world.Settlements.Weaken(cell, 1);
                    world.Memory.Add(cell, "beast-raid");
                }
                else if (beasts > 0 && population < world.Settlements.CapacityOf(cell) + _allyBonus)
                    world.Settlements.Bless(cell, 1);
            }
        }

        public void End(CatastropheContext context) { }
    }

    [Serializable]
    public sealed class AncestorLaw : IAgeLaw
    {
        [SerializeField, Required, AssetsOnly, InfoBox("Во что превращается место брошенного поселения.")] private TileDefinition _mound;

        [NonSerialized] private CatastropheContext _context;
        [NonSerialized] private string _cause;

        public void Begin(CatastropheContext context, string cause)
        {
            _context = context;
            _cause = cause;
            context.World.Settlements.Abandoned += OnAbandoned;
        }

        public void Year(CatastropheContext context, int age, string cause) { }

        public void End(CatastropheContext context) => context.World.Settlements.Abandoned -= OnAbandoned;

        private void OnAbandoned(GridCoord cell)
        {
            var world = _context.World;
            if (!_context.IsShaped(cell) || world.Guards.IsProtected(cell)) return;

            world.Tiles.Place(cell, _mound.Id, _cause);
            world.Memory.Add(cell, "ancestors");
        }
    }

    [Serializable]
    public sealed class ForgetLaw : IAgeLaw
    {
        [SerializeField, MinValue(1), SuffixLabel("years")] private int _period = 300;

        public void Begin(CatastropheContext context, string cause) { }

        public void Year(CatastropheContext context, int age, string cause)
        {
            if (age % _period != 0) return;

            var world = context.World;
            world.Memory.ForgetAll(world.Grid.Center, world.Grid.Width + world.Grid.Height);
        }

        public void End(CatastropheContext context) { }
    }

    [Serializable]
    public sealed class TideLaw : IAgeLaw
    {
        [SerializeField, MinValue(1), SuffixLabel("years"), InfoBox("Раз в период край мира уходит под воду, в следующий — возвращается.")] private int _period = 100;
        [SerializeField, Required, AssetsOnly] private TileDefinition _water;

        [NonSerialized] private List<(GridCoord Cell, string TileId)> _flooded = new();

        public void Begin(CatastropheContext context, string cause) => _flooded = new List<(GridCoord, string)>();

        public void Year(CatastropheContext context, int age, string cause)
        {
            if (age % _period != 0) return;

            var world = context.World;
            if (_flooded.Count > 0)
            {
                foreach (var (cell, tileId) in _flooded)
                    if (world.Tiles.TryGet(cell, out var current) && current == _water.Id)
                        world.Tiles.Place(cell, tileId, "отлив");
                _flooded.Clear();
                return;
            }

            var tiles = world.Tiles.All.Where(pair => context.IsShaped(pair.Key) && pair.Value != _water.Id && !world.Guards.IsProtected(pair.Key)).ToList();
            if (tiles.Count == 0) return;

            var sides = new[] { (1, 0), (-1, 0), (0, 1), (0, -1) };
            var (x, y) = sides[world.Random.Next(sides.Length)];
            var edge = tiles.Max(pair => pair.Key.X * x + pair.Key.Y * y);
            foreach (var (cell, tileId) in tiles.Where(pair => pair.Key.X * x + pair.Key.Y * y == edge).ToList())
            {
                _flooded.Add((cell, tileId));
                world.Tiles.Place(cell, _water.Id, cause);
            }
        }

        public void End(CatastropheContext context) { }
    }

    [Serializable]
    public sealed class CityGodsLaw : IAgeLaw
    {
        [SerializeField, MinValue(1), SuffixLabel("years")] private int _period = 50;
        [SerializeField, MinValue(1)] private int _minPopulation = 10;
        [SerializeField, Required, InfoBox("Id шаблонов богов (talas, urgar, veila, ifrit).")] private List<string> _templates = new();

        public void Begin(CatastropheContext context, string cause) { }

        public void Year(CatastropheContext context, int age, string cause)
        {
            if (age % _period != 0 || _templates.Count == 0) return;

            var world = context.World;
            var city = context.Cities.All
                .Where(cell => world.Settlements.TryGet(cell, out var population) && population >= _minPopulation && !world.Pantheon.TryGetAt(cell, out _))
                .OrderByDescending(cell => world.Settlements.All[cell]).ThenBy(cell => cell.X).ThenBy(cell => cell.Y)
                .Cast<GridCoord?>().FirstOrDefault();
            if (city is not { } seat) return;

            var god = world.Pantheon.Manifest(seat, _templates[world.Random.Next(_templates.Count)], "Город-бог");
            if (god != null) world.History.Note(seat, $"Город стал богом ({cause})");
        }

        public void End(CatastropheContext context) { }
    }

    [Serializable]
    public sealed class EndlessWarLaw : IAgeLaw
    {
        [SerializeField, MinValue(1), SuffixLabel("years")] private int _period = 40;

        public void Begin(CatastropheContext context, string cause) { }

        public void Year(CatastropheContext context, int age, string cause)
        {
            if (age % _period != 0) return;

            var realms = context.World.Realms;
            foreach (var kingdom in realms.All.Where(kingdom => !realms.IsAtWar(kingdom)).ToList())
            {
                var rival = realms.All.FirstOrDefault(other => other != kingdom && !realms.IsAtWar(other));
                if (rival != null) realms.TryDeclareWar(kingdom, rival);
            }
        }

        public void End(CatastropheContext context) { }
    }

    [Serializable]
    public sealed class ReverseTimeLaw : IAgeLaw
    {
        [SerializeField, MinValue(1), SuffixLabel("years")] private int _period = 25;
        [SerializeField, MinValue(1)] private int _cells = 2;

        public void Begin(CatastropheContext context, string cause) { }

        public void Year(CatastropheContext context, int age, string cause)
        {
            if (age % _period != 0) return;

            var world = context.World;
            var reverts = new List<(GridCoord Cell, string TileId)>();
            foreach (var (cell, tileId) in world.Tiles.All)
            {
                if (!context.IsShaped(cell) || world.Guards.IsProtected(cell)) continue;

                var previous = world.History.Of(cell).Reverse()
                    .Select(record => record.TileId)
                    .FirstOrDefault(id => !string.IsNullOrEmpty(id) && id != tileId);
                if (previous != null) reverts.Add((cell, previous));
            }

            foreach (var (cell, tileId) in reverts.OrderBy(pair => pair.Cell.X).ThenBy(pair => pair.Cell.Y).OrderBy(_ => world.Random.Next(100000)).Take(_cells))
                world.Tiles.Place(cell, tileId, cause);
        }

        public void End(CatastropheContext context) { }
    }

    [Serializable]
    public sealed class TitanBackLaw : IAgeLaw
    {
        [SerializeField, MinValue(1), SuffixLabel("years"), InfoBox("Раз в период мир сдвигается: один край осыпается в пустоту, у противоположного встаёт новая земля.")] private int _period = 200;
        [SerializeField, Required, AssetsOnly] private TileDefinition _land;
        [SerializeField, MinValue(1)] private int _newCells = 5;

        public void Begin(CatastropheContext context, string cause) { }

        public void Year(CatastropheContext context, int age, string cause)
        {
            if (age % _period != 0) return;

            var world = context.World;
            var sides = new[] { new GridCoord(1, 0), new GridCoord(-1, 0), new GridCoord(0, 1), new GridCoord(0, -1) };
            var side = sides[world.Random.Next(sides.Length)];
            var tiles = world.Tiles.All.Keys.Where(context.IsShaped).ToList();
            if (tiles.Count == 0) return;

            var edge = tiles.Max(cell => cell.X * side.X + cell.Y * side.Y);
            foreach (var cell in tiles.Where(cell => cell.X * side.X + cell.Y * side.Y == edge && !world.Guards.IsProtected(cell)).ToList())
                world.Tiles.Remove(cell, cause);

            var before = world.Grid.Coords.Where(world.Grid.IsUnlocked).ToHashSet();
            world.Grid.UnlockSide(new GridCoord(-side.X, -side.Y), _newCells);
            foreach (var cell in world.Grid.Coords.Where(cell => world.Grid.IsUnlocked(cell) && !before.Contains(cell)).ToList())
                if (!world.Tiles.All.ContainsKey(cell)) world.Tiles.Place(cell, _land.Id, cause);
        }

        public void End(CatastropheContext context) { }
    }

    [Serializable]
    public sealed class TitanDustLaw : IAgeLaw
    {
        [SerializeField, Required, AssetsOnly, InfoBox("Во что превращается тело титана.")] private TileDefinition _remains;

        public void Begin(CatastropheContext context, string cause) => Petrify(context, cause);

        public void Year(CatastropheContext context, int age, string cause) => Petrify(context, cause);

        public void End(CatastropheContext context) { }

        private void Petrify(CatastropheContext context, string cause)
        {
            var world = context.World;
            foreach (var titan in world.Titans.All.Where(titan => titan.IsAwake).ToList())
            {
                var cell = titan.Position;
                world.Titans.Slay(titan, "Прах титанов");
                if (world.Tiles.All.ContainsKey(cell) && cell != world.Grid.Center) world.Tiles.Place(cell, _remains.Id, cause);
            }
        }
    }

    [Serializable]
    public sealed class BloodLaw : IAgeLaw
    {
        [SerializeField, MinValue(1), SuffixLabel("years")] private int _period = 40;
        [SerializeField, InfoBox("Смешение: соседние народы сливаются в больший. Иначе — чистота крови: соседи разных народов враждуют.")] private bool _mix = true;

        public void Begin(CatastropheContext context, string cause) { }

        public void Year(CatastropheContext context, int age, string cause)
        {
            if (age % _period != 0) return;

            var world = context.World;
            var races = context.Races;
            if (_mix)
            {
                var living = races.Living.OrderByDescending(race => race.Cells.Count).ToList();
                foreach (var (big, small) in living.SelectMany(big => living.Where(small => small != big && small.Cells.Count <= big.Cells.Count).Select(small => (big, small))))
                {
                    if (!big.IsAlive || !small.IsAlive || !small.Cells.Any(cell => world.Grid.Neighbors(cell).Any(big.Cells.Contains))) continue;

                    races.Absorb(big, small);
                    big.Cells.UnionWith(small.Cells);
                    small.Cells.Clear();
                    world.Memory.Add(world.Grid.Center, "blood-mixed");
                    return;
                }

                return;
            }

            foreach (var (cell, population) in world.Settlements.All.ToList())
            {
                var race = races.RaceAt(cell);
                if (race == null) continue;

                foreach (var neighbor in world.Grid.Neighbors(cell))
                {
                    var other = races.RaceAt(neighbor);
                    if (other == null || other == race || !world.Settlements.TryGet(neighbor, out var theirs) || theirs > population) continue;

                    world.Settlements.Weaken(neighbor, 1);
                    world.Memory.Add(world.Grid.Center, "race-war");
                }
            }
        }

        public void End(CatastropheContext context) { }
    }
}
