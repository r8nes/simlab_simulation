using System;
using System.Linq;
using Godland.Core.World.Cells;
using Godland.Core.World.Grid;
using Godland.Core.Randomness;
using Godland.Core.World.Tiles;

namespace Godland.Core.Life.Features
{
    public sealed class FeatureSpread
    {
        private readonly WorldGrid _grid;
        private readonly WorldTiles _tiles;
        private readonly TileFeatures _features;
        private readonly FeatureDepletion _depletion;
        private readonly IFeatureRules _rules;
        private readonly SeededRandom _random;
        private readonly CellFertility _fertility;

        public FeatureSpread(
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

        public event Action<GridCoord, GridCoord, string> Spread;

        private bool IsBarren(GridCoord coord) => _fertility.CanHold(coord) && _fertility.Get(coord) < _rules.MinFertilityToThrive;

        private readonly System.Collections.Generic.Dictionary<GridCoord, int> _energy = new();

        private Func<GridCoord, int> _extraBirths = _ => 0;
        private Func<GridCoord, bool> _hardy = _ => false;

        public void SetBreeding(Func<GridCoord, int> extraBirths, Func<GridCoord, bool> hardy)
        {
            _extraBirths = extraBirths;
            _hardy = hardy;
        }

        public int EnergyOf(GridCoord colony) => _energy.TryGetValue(colony, out var energy) ? energy : 0;

        public void Update(int year)
        {
            Feed();
            if (year % _rules.SpreadPeriod != 0) return;

            foreach (var (colony, featureId) in _features.All.ToList())
            {
                if (_features.IsWild(colony) || _depletion.IsDepleted(colony) || IsBarren(colony)) continue;

                var births = Math.Min(2 + _extraBirths(colony), EnergyOf(colony) / _rules.SpreadEnergy + _extraBirths(colony));
                for (var i = 0; i < births; i++)
                {
                    var targets = _grid.Neighbors(colony)
                        .Where(cell => !_features.TryGet(cell, out _) && !IsBarren(cell) && _tiles.TryGet(cell, out var tileId) && (_rules.CanLiveOn(featureId, tileId) || _hardy(colony)))
                        .ToList();
                    if (targets.Count == 0) break;

                    var target = targets[_random.Next(targets.Count)];
                    _features.AddWild(target, featureId);
                    _energy[colony] = Math.Max(0, EnergyOf(colony) - _rules.SpreadEnergy);
                    Spread?.Invoke(colony, target, featureId);
                }
            }
        }

        private void Feed()
        {
            foreach (var colony in _energy.Keys.Where(colony => !_features.TryGet(colony, out _)).ToList()) _energy.Remove(colony);
            foreach (var colony in _features.All.Keys.ToList())
            {
                if (_features.IsWild(colony)) continue;
                if (_depletion.IsDepleted(colony))
                {
                    _energy[colony] = 0;
                    continue;
                }

                var gain = _fertility.CanHold(colony) ? _fertility.Get(colony) : _rules.MinFertilityToThrive;
                _energy[colony] = Math.Min(_rules.SpreadEnergy * 2, EnergyOf(colony) + gain);
            }
        }
    }
}
