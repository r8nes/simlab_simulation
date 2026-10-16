using System;
using System.Collections.Generic;
using Godland.Core.Fate.Epochs;
using Godland.Core.World.Grid;
using Godland.Core.World.Tiles;

namespace Godland.Core.World.Cells
{
    public sealed class FertilityOverflow
    {
        private readonly WorldTiles _tiles;
        private readonly TileGuards _guards;
        private readonly IOverflowRules _rules;
        private readonly WorldEpoch _epoch;
        private readonly Dictionary<GridCoord, int> _excess = new();

        public FertilityOverflow(CellFertility fertility, WorldTiles tiles, TileGuards guards, IOverflowRules rules, WorldEpoch epoch)
        {
            _tiles = tiles;
            _guards = guards;
            _rules = rules;
            _epoch = epoch;

            fertility.Overflowed += OnOverflowed;
            _tiles.Cleared += coord => _excess.Remove(coord);
        }

        public event Action<GridCoord, string> Transformed;

        public bool IsEnabled { get; private set; }

        public void SetEnabled(bool enabled) => IsEnabled = enabled;

        public int ExcessAt(GridCoord coord) => _excess.GetValueOrDefault(coord);

        private void OnOverflowed(GridCoord coord, int amount)
        {
            if (!IsEnabled || !_tiles.TryGet(coord, out var tileId) || _guards.IsProtected(coord)) return;

            var result = _rules.OverflowResult(tileId, _epoch.Index);
            if (result == null) return;

            var excess = _excess.GetValueOrDefault(coord) + amount;
            if (excess < _rules.ThresholdFor(tileId, _epoch.Index))
            {
                _excess[coord] = excess;
                return;
            }

            _tiles.Place(coord, result, "избыток плодородия");
            Transformed?.Invoke(coord, result);
        }
    }
}
