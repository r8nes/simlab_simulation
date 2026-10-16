using System;
using System.Linq;
using Godland.Core.World.Cells;
using Godland.Core.World.Grid;
using Godland.Core.Randomness;
using Godland.Core.World.Tiles;

namespace Godland.Core.Life.Features
{
    public sealed class FeatureEvolutions
    {
        private readonly WorldGrid _grid;
        private readonly WorldTiles _tiles;
        private readonly TileFeatures _features;
        private readonly FeatureDepletion _depletion;
        private readonly IFeatureRules _rules;
        private readonly SeededRandom _random;
        private readonly CellFertility _fertility;

        public FeatureEvolutions(
            WorldGrid grid, WorldTiles tiles, TileFeatures features, FeatureDepletion depletion, IFeatureRules rules, SeededRandom random,
            CellFertility fertility)
        {
            _grid = grid;
            _tiles = tiles;
            _features = features;
            _depletion = depletion;
            _rules = rules;
            _random = random;
            _fertility = fertility;
        }

        public event Action<GridCoord, string, string> Evolved;

        private bool IsBarren(GridCoord coord) => _fertility.CanHold(coord) && _fertility.Get(coord) < _rules.MinFertilityToThrive;

        public void Update(int year)
        {
            if (year % _rules.EvolutionPeriod != 0) return;

            foreach (var (coord, featureId) in _features.All.ToList())
            {
                if (_depletion.IsDepleted(coord) || IsBarren(coord)) continue;

                foreach (var rule in _rules.EvolutionsOf(featureId))
                {
                    if (!Holds(coord, rule) || _random.NextFloat() >= rule.Chance) continue;

                    _features.Replace(coord, rule.ResultId, _features.IsWild(coord));
                    Evolved?.Invoke(coord, featureId, rule.ResultId);
                    break;
                }
            }
        }

        private bool Holds(GridCoord coord, FeatureEvolutionRule rule) =>
            (rule.NeighborTileId == null || _grid.Neighbors(coord).Any(cell => _tiles.TryGet(cell, out var id) && id == rule.NeighborTileId))
            && (rule.NeighborFeatureId == null || _grid.Neighbors(coord).Any(cell => _features.TryGet(cell, out var id) && id == rule.NeighborFeatureId));
    }
}
