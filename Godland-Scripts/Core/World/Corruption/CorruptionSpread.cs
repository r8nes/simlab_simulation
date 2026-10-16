using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Grid;
using Godland.Core.Life.People;
using Godland.Core.Randomness;
using Godland.Core.World.Tiles;

namespace Godland.Core.World.Corruption
{
    public sealed class CorruptionSpread
    {
        private readonly WorldGrid _grid;
        private readonly WorldTiles _tiles;
        private readonly Settlements _settlements;
        private readonly ICorruptionRules _rules;
        private readonly SeededRandom _random;
        private readonly WorldModifiers _modifiers;
        private readonly TileGuards _guards;
        private readonly Godland.Core.World.Patterns.GrowthPatterns _patterns;

        public CorruptionSpread(WorldGrid grid, WorldTiles tiles, Settlements settlements, ICorruptionRules rules, SeededRandom random, WorldModifiers modifiers,
            TileGuards guards, Godland.Core.World.Patterns.GrowthPatterns patterns)
        {
            _patterns = patterns;
            _grid = grid;
            _tiles = tiles;
            _settlements = settlements;
            _rules = rules;
            _random = random;
            _modifiers = modifiers;
            _guards = guards;
        }

        public event Action<GridCoord> Corrupted;
        public event Action<GridCoord> Healed;

        public bool IsTainted(GridCoord coord) =>
            _tiles.TryGet(coord, out var id) && (_rules.IsSource(id) || id == _rules.CorruptedTileId);

        public void Update(int year)
        {
            if (year % _rules.CorruptionPeriod != 0) return;

            var tainted = _tiles.All.Keys.Where(IsTainted).ToList();
            if (tainted.Count == 0) return;

            foreach (var cell in tainted)
                _settlements.Weaken(cell, _rules.CorruptionPopulationLoss);

            if (!tainted.Any(cell => _tiles.TryGet(cell, out var id) && _rules.IsSource(id)))
            {
                Heal(tainted);
                return;
            }

            if (_random.Next(100) >= _rules.CorruptionChancePercent * _modifiers.CorruptionChanceMultiplier) return;

            var frontier = tainted.SelectMany(_grid.Neighbors).Distinct()
                .Where(cell => _tiles.TryGet(cell, out var id) && !IsTainted(cell) && !_rules.ResistsCorruption(id) && !_guards.IsProtected(cell))
                .ToList();
            if (frontier.Count == 0) return;

            var target = _patterns.Rank("corruption", Godland.Core.World.Patterns.PatternStyle.Tongues, tainted, frontier).First();
            _tiles.Place(target, _rules.CorruptedTileId, "порча");
            Corrupted?.Invoke(target);
        }

        private void Heal(IReadOnlyList<GridCoord> tainted)
        {
            var target = tainted[_random.Next(tainted.Count)];
            _tiles.Place(target, _rules.HealedTileId, "порча отступила");
            Healed?.Invoke(target);
        }
    }
}
