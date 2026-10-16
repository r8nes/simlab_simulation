using System;
using System.Collections.Generic;
using Godland.Core.Life.Features;
using Godland.Core.World.Grid;
using Godland.Core.World.Tiles;

namespace Godland.Core.World.Creation
{
    public sealed class CreationYield
    {
        private readonly WorldGrid _grid;
        private readonly WorldTiles _tiles;
        private readonly ITileRules _tileRules;
        private readonly TileFeatures _features;
        private readonly IFeatureRules _featureRules;
        private readonly CreationPoints _creation;
        private readonly Dictionary<GridCoord, int> _tileAges = new();
        private readonly Dictionary<GridCoord, int> _featureAges = new();

        public CreationYield(
            WorldGrid grid, WorldTiles tiles, ITileRules tileRules, TileFeatures features, IFeatureRules featureRules, CreationPoints creation)
        {
            _grid = grid;
            _tiles = tiles;
            _tileRules = tileRules;
            _features = features;
            _featureRules = featureRules;
            _creation = creation;

            _tiles.Cleared += coord => _tileAges.Remove(coord);
            _features.Added += (coord, _) => _featureAges.Remove(coord);
        }

        public event Action<GridCoord, int> Yielded;

        public void Collect()
        {
            foreach (var coord in _grid.Coords)
            {
                if (_tiles.TryGet(coord, out var tileId))
                {
                    var age = Grow(_tileAges, coord);
                    if (_tileRules.TryGetCreationYield(tileId, out var amount, out var period)) TryYield(coord, amount, period, age);
                }

                if (_features.TryGet(coord, out var featureId) && !_features.IsWild(coord))
                {
                    var age = Grow(_featureAges, coord);
                    if (_featureRules.TryGetCreationYield(featureId, out var amount, out var period)) TryYield(coord, amount, period, age);
                }
            }
        }

        private static int Grow(Dictionary<GridCoord, int> ages, GridCoord coord) => ages[coord] = ages.GetValueOrDefault(coord) + 1;

        private void TryYield(GridCoord coord, int amount, int period, int age)
        {
            if (age % period != 0) return;

            _creation.Add(amount);
            Yielded?.Invoke(coord, amount);
        }
    }
}
