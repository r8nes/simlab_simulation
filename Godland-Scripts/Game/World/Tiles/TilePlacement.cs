using System;
using Godland.Core.World.Creation;
using Godland.Core.World.Grid;
using Godland.Core.World.Tiles;

namespace Godland.Game.World.Tiles
{
    public sealed class TilePlacement
    {
        private readonly WorldTiles _tiles;
        private readonly CreationPoints _creation;
        private readonly TileBrush _brush;
        private readonly PlayerLocks _locks;

        public TilePlacement(WorldTiles tiles, CreationPoints creation, TileBrush brush, PlayerLocks locks)
        {
            _locks = locks;
            _tiles = tiles;
            _creation = creation;
            _brush = brush;
        }

        public event Action<GridCoord, TileDefinition> Placed;

        public bool CanPlace(GridCoord coord) => !_locks.IsLocked(coord) && _tiles.CanPlace(coord) && _creation.CanAfford(_brush.Current.Cost);

        public bool TryPlace(GridCoord coord)
        {
            if (!CanPlace(coord) || !_creation.TrySpend(_brush.Current.Cost)) return false;

            _tiles.PlayerActing = true;
            try { _tiles.Place(coord, _brush.Current.Id, "поставлено тобой"); }
            finally { _tiles.PlayerActing = false; }
            Placed?.Invoke(coord, _brush.Current);
            return true;
        }
    }
}
