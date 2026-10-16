using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Grid;
using Godland.Core.World.Tiles;

namespace Godland.Core.Life.Features
{
    public sealed class FeatureDepletion
    {
        private readonly Dictionary<GridCoord, int> _yearsLeft = new();

        public FeatureDepletion(WorldTiles tiles, TileFeatures features)
        {
            tiles.Cleared += Forget;
            features.Removed += (coord, _) => Forget(coord);
        }

        public event Action<GridCoord> Depleted;
        public event Action<GridCoord> Recovered;

        public bool IsDepleted(GridCoord coord) => _yearsLeft.ContainsKey(coord);

        public bool TryGet(GridCoord coord, out int yearsLeft) => _yearsLeft.TryGetValue(coord, out yearsLeft);

        public void Deplete(GridCoord coord, int years)
        {
            if (years <= 0) return;

            var wasDepleted = _yearsLeft.ContainsKey(coord);
            _yearsLeft[coord] = years;
            if (!wasDepleted) Depleted?.Invoke(coord);
        }

        public void Advance()
        {
            foreach (var coord in _yearsLeft.Keys.ToList())
            {
                if (--_yearsLeft[coord] > 0) continue;

                _yearsLeft.Remove(coord);
                Recovered?.Invoke(coord);
            }
        }

        private void Forget(GridCoord coord) => _yearsLeft.Remove(coord);
    }
}
