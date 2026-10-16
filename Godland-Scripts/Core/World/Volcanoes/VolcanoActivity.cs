using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.Life.Features;
using Godland.Core.World.Grid;
using Godland.Core.Life.People;
using Godland.Core.Randomness;
using Godland.Core.World.Tiles;

namespace Godland.Core.World.Volcanoes
{
    public sealed class VolcanoActivity
    {
        private readonly WorldGrid _grid;
        private readonly WorldTiles _tiles;
        private readonly TileFeatures _features;
        private readonly FeatureDepletion _depletion;
        private readonly IFeatureRules _featureRules;
        private readonly Settlements _settlements;
        private readonly Shrines _shrines;
        private readonly IVolcanoRules _rules;
        private readonly SeededRandom _random;
        private readonly TileGuards _guards;
        private readonly Dictionary<GridCoord, int> _awakening = new();

        public VolcanoActivity(
            WorldGrid grid, WorldTiles tiles, TileFeatures features, FeatureDepletion depletion, IFeatureRules featureRules,
            Settlements settlements, Shrines shrines, IVolcanoRules rules, SeededRandom random, TileGuards guards)
        {
            _grid = grid;
            _tiles = tiles;
            _features = features;
            _depletion = depletion;
            _featureRules = featureRules;
            _settlements = settlements;
            _shrines = shrines;
            _rules = rules;
            _random = random;
            _guards = guards;

            _tiles.Cleared += coord => _awakening.Remove(coord);
        }

        public event Action<GridCoord, int> Awakened;
        public event Action<GridCoord, IReadOnlyList<GridCoord>> Erupted;
        public event Action<GridCoord, GridCoord> Melted;

        public IReadOnlyDictionary<GridCoord, int> Awakenings => _awakening;

        public bool TryGetAwakening(GridCoord volcano, out int yearsLeft) => _awakening.TryGetValue(volcano, out yearsLeft);

        public bool CanAwaken(GridCoord volcano) =>
            _tiles.TryGet(volcano, out var id) && _rules.IsVolcano(id) && !_awakening.ContainsKey(volcano);

        public void Awaken(GridCoord volcano)
        {
            if (!CanAwaken(volcano)) return;

            _awakening[volcano] = _rules.WarningYears / 4 + 1;
            Awakened?.Invoke(volcano, _awakening[volcano]);
        }

        public void EruptNow(GridCoord volcano) => Erupt(volcano);

        public void AwakenAll()
        {
            foreach (var volcano in Volcanoes)
            {
                if (_awakening.ContainsKey(volcano)) continue;

                _awakening[volcano] = _rules.WarningYears / 4 + 1;
                Awakened?.Invoke(volcano, _awakening[volcano]);
            }
        }

        public void Update(int year)
        {
            AdvanceAwakenings();
            if (year % _rules.CheckPeriod == 0) RollEruptions();
            if (year % _rules.MeltPeriod == 0) MeltSnow();
        }

        private IEnumerable<GridCoord> Volcanoes =>
            _tiles.All.Where(pair => _rules.IsVolcano(pair.Value)).Select(pair => pair.Key).ToList();

        private void RollEruptions()
        {
            foreach (var volcano in Volcanoes)
            {
                if (_awakening.ContainsKey(volcano)) continue;

                var chance = _rules.EruptionChancePercent + _shrines.BelieversOf(volcano) * _rules.ChancePerBelieverPercent;
                if (_random.Next(100) >= chance) continue;

                _awakening[volcano] = _rules.WarningYears;
                Awakened?.Invoke(volcano, _rules.WarningYears);
            }
        }

        private void AdvanceAwakenings()
        {
            foreach (var volcano in _awakening.Keys.ToList())
            {
                if (!_awakening.ContainsKey(volcano) || --_awakening[volcano] > 0) continue;

                _awakening.Remove(volcano);
                Erupt(volcano);
            }
        }

        private void Erupt(GridCoord volcano)
        {
            var burned = new List<GridCoord>();
            foreach (var cell in _grid.Neighbors(volcano))
            {
                if (_features.TryGet(cell, out _)) _depletion.Deplete(cell, _featureRules.RecoveryYears);
                _settlements.Weaken(cell, _rules.PopulationLoss);

                if (!_tiles.TryGet(cell, out var tileId) || !_rules.BurnsToAsh(tileId) || _guards.IsProtected(cell)) continue;

                _tiles.Place(cell, _rules.AshTileId, "вулкан");
                burned.Add(cell);
            }

            Erupted?.Invoke(volcano, burned);
        }

        private void MeltSnow()
        {
            foreach (var volcano in Volcanoes)
            {
                var snow = _grid.Neighbors(volcano).Where(cell => _tiles.TryGet(cell, out var id) && _rules.Melts(id)).ToList();
                if (snow.Count == 0) continue;

                var target = snow[_random.Next(snow.Count)];
                _tiles.Place(target, _rules.MeltedTileId, "вулкан");
                Melted?.Invoke(volcano, target);
            }
        }
    }
}
