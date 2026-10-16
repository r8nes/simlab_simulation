using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Cells;
using Godland.Core.Fate.Epochs;
using Godland.Core.Life.Features;
using Godland.Core.World.Grid;
using Godland.Core.Randomness;
using Godland.Core.World.Tiles;

namespace Godland.Core.World.Climate
{
    public sealed class WorldClimate
    {
        private readonly WorldGrid _grid;
        private readonly WorldTiles _tiles;
        private readonly CellFertility _fertility;
        private readonly IClimateRules _rules;
        private readonly SeededRandom _random;
        private readonly Dictionary<GridCoord, int> _dryYears = new();
        private readonly TileFeatures _features;
        private readonly IFeatureRules _featureRules;
        private readonly WorldEpoch _epoch;
        private readonly TileGuards _guards;
        private readonly HashSet<GridCoord> _wasFertile = new();

        public WorldClimate(WorldGrid grid, WorldTiles tiles, CellFertility fertility, IClimateRules rules, SeededRandom random,
            TileFeatures features, IFeatureRules featureRules, WorldEpoch epoch, TileGuards guards)
        {
            _grid = grid;
            _tiles = tiles;
            _fertility = fertility;
            _rules = rules;
            _random = random;
            _features = features;
            _featureRules = featureRules;
            _epoch = epoch;
            _guards = guards;

            _tiles.Cleared += coord =>
            {
                _dryYears.Remove(coord);
                _wasFertile.Remove(coord);
            };
            _fertility.Changed += (coord, value) =>
            {
                if (value > 0) _wasFertile.Add(coord);
            };
        }

        public event Action<GridCoord> Desertified;
        public event Action<GridCoord, GridCoord> Frozen;
        public event Action<GridCoord> ForestGrew;
        public event Action<GridCoord> SeaSpread;
        public event Action<GridCoord, string> WildlifeBorn;

        public int DryYears(GridCoord coord) => _dryYears.GetValueOrDefault(coord);

        public void Update(int year)
        {
            Dry();
            if (year % _rules.GlacierPeriod == 0) Freeze();
            if (year % _rules.ForestSpreadPeriod == 0) GrowForests();
            if (_rules.SeaTileId != null && year % _rules.SeaSpreadPeriod == 0) GrowSeas();
            if (year % _rules.WildlifePeriod == 0) BreedWildlife();
        }

        private void Dry()
        {
            foreach (var (cell, tileId) in _tiles.All.ToList())
            {
                if (!IsParched(cell, tileId))
                {
                    _dryYears.Remove(cell);
                    continue;
                }

                var years = _dryYears.GetValueOrDefault(cell) + 1;
                if (years < _rules.DesertYears)
                {
                    _dryYears[cell] = years;
                    continue;
                }

                _tiles.Place(cell, _rules.DesertTileId, "засуха");
                Desertified?.Invoke(cell);
            }
        }

        private bool IsParched(GridCoord cell, string tileId) =>
            _epoch.Index >= _rules.DesertFromEpoch && _wasFertile.Contains(cell) && !_guards.IsProtected(cell)
            && _rules.CanDryOut(tileId)
            && _fertility.CanHold(cell) && _fertility.Get(cell) == 0
            && !_grid.Neighbors(cell).Any(neighbor => _tiles.TryGet(neighbor, out var id) && _rules.IsWater(id));

        private void GrowSeas()
        {
            foreach (var (sea, tileId) in _tiles.All.ToList())
            {
                if (tileId != _rules.SeaTileId || _random.Next(100) >= _rules.SeaSpreadChancePercent) continue;

                var water = _grid.Neighbors(sea).Where(cell => _tiles.TryGet(cell, out var id) && _rules.IsAbsorbedBySea(id) && !_guards.IsProtected(cell)).ToList();
                if (water.Count == 0) continue;

                var target = water[_random.Next(water.Count)];
                _tiles.Place(target, _rules.SeaTileId, "море поглотило воду");
                SeaSpread?.Invoke(target);
            }
        }

        private void GrowForests()
        {
            foreach (var (forest, tileId) in _tiles.All.ToList())
            {
                if (!_rules.IsSpreadingForest(tileId) || _fertility.Get(forest) < _fertility.Max) continue;
                if (_random.Next(100) >= _rules.ForestSpreadChancePercent) continue;

                var clearings = _grid.Neighbors(forest).Where(cell => _tiles.TryGet(cell, out var id) && _rules.CanBecomeForest(id) && !_guards.IsProtected(cell)).ToList();
                if (clearings.Count == 0) continue;

                var target = clearings[_random.Next(clearings.Count)];
                _tiles.Place(target, _rules.ForestTileId, "лес разросся");
                ForestGrew?.Invoke(target);
            }
        }

        private void BreedWildlife()
        {
            foreach (var (cell, tileId) in _tiles.All.ToList())
            {
                if (!_rules.BreedsWildlife(tileId) || _features.TryGet(cell, out _)) continue;

                var species = _rules.WildlifeIds.Where(id => _featureRules.CanLiveOn(id, tileId)).ToList();
                if (species.Count == 0) species = _rules.WildlifeIds.ToList();
                if (species.Count == 0) continue;

                var featureId = species[_random.Next(species.Count)];
                _features.AddWild(cell, featureId);
                WildlifeBorn?.Invoke(cell, featureId);
            }
        }

        private void Freeze()
        {
            foreach (var (peak, tileId) in _tiles.All.ToList())
            {
                if (!_rules.IsPeak(tileId)) continue;

                var neighbors = _grid.Neighbors(peak).ToList();
                if (neighbors.Count(cell => _tiles.TryGet(cell, out var id) && _rules.IsPeak(id)) < _rules.GlacierMinPeaks) continue;

                var lowlands = neighbors.Where(cell => _tiles.TryGet(cell, out var id) && _rules.CanFreeze(id) && !_guards.IsProtected(cell)).ToList();
                if (lowlands.Count == 0) continue;

                var target = lowlands[_random.Next(lowlands.Count)];
                _tiles.Place(target, _rules.SnowTileId, "ледник");
                Frozen?.Invoke(peak, target);
            }
        }
    }
}
