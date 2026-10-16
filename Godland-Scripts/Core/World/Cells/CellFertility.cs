using System;
using System.Collections.Generic;
using Godland.Core.Life.Features;
using Godland.Core.World.Grid;
using Godland.Core.World.Tiles;

namespace Godland.Core.World.Cells
{
    public sealed class CellFertility
    {
        private readonly WorldGrid _grid;
        private readonly WorldTiles _tiles;
        private readonly ITileRules _rules;
        private readonly TileFeatures _features;
        private readonly IFeatureRules _featureRules;
        private readonly WorldModifiers _modifiers;
        private readonly int _maxFertility;
        private readonly int _growthPeriod;
        private readonly Dictionary<GridCoord, int> _values = new();

        public CellFertility(WorldGrid grid, WorldTiles tiles, ITileRules rules, TileFeatures features, IFeatureRules featureRules, WorldModifiers modifiers, int maxFertility, int growthPeriod)
        {
            if (maxFertility <= 0) throw new ArgumentOutOfRangeException(nameof(maxFertility));
            if (growthPeriod <= 0) throw new ArgumentOutOfRangeException(nameof(growthPeriod));

            _grid = grid;
            _tiles = tiles;
            _rules = rules;
            _features = features;
            _featureRules = featureRules;
            _modifiers = modifiers;
            _maxFertility = maxFertility;
            _growthPeriod = growthPeriod;

            _tiles.Cleared += Reset;
        }

        public event Action<GridCoord, int> Changed;
        public event Action<GridCoord, int> Overflowed;

        public int Max => _maxFertility;

        public int Get(GridCoord coord) => _values.GetValueOrDefault(coord);

        public bool CanHold(GridCoord coord) => _tiles.TryGet(coord, out var tileId) && _rules.HasFertility(tileId);

        public void Grow(int year)
        {
            if (_modifiers.IsNight(year)) return;

            if (year % _growthPeriod != 0) return;

            var gains = new Dictionary<GridCoord, int>();
            foreach (var (coord, tileId) in _tiles.All)
                AddToNeighbors(gains, coord, _rules.GetNeighborFertility(tileId));

            foreach (var (coord, featureId) in _features.All)
            {
                if (_features.IsWild(coord)) continue;

                AddToNeighbors(gains, coord, _featureRules.GetNeighborFertility(featureId));
                gains[coord] = gains.GetValueOrDefault(coord) + _featureRules.GetSelfFertility(featureId);
            }

            foreach (var coord in _grid.Coords)
                if (gains.TryGetValue(coord, out var gain))
                    Add(coord, gain);
        }

        private void AddToNeighbors(Dictionary<GridCoord, int> gains, GridCoord source, int amount)
        {
            if (amount == 0) return;

            foreach (var neighbor in _grid.Neighbors(source))
                if (_tiles.TryGet(neighbor, out _))
                    gains[neighbor] = gains.GetValueOrDefault(neighbor) + amount;
        }

        private void Reset(GridCoord coord)
        {
            if (!_values.Remove(coord, out var previous) || previous == 0) return;
            Changed?.Invoke(coord, 0);
        }

        public void Add(GridCoord coord, int amount)
        {
            if (!CanHold(coord)) return;

            var current = Get(coord);
            var next = Math.Clamp(current + amount, 0, _maxFertility);
            if (next != current)
            {
                _values[coord] = next;
                Changed?.Invoke(coord, next);
            }

            if (current + amount > _maxFertility) Overflowed?.Invoke(coord, current + amount - _maxFertility);
        }
    }
}
