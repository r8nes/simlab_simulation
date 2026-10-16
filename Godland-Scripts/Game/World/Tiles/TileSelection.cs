using System;
using Godland.Core.World.Grid;
using Godland.Core.World.Tiles;

namespace Godland.Game.World.Tiles
{
    public sealed class TileSelection
    {
        public TileSelection(WorldTiles tiles)
        {
            tiles.Removed += coord =>
            {
                if (Selected == coord) Clear();
            };
        }

        public event Action Changed;

        public GridCoord? Selected { get; private set; }

        public void Select(GridCoord coord)
        {
            if (Selected == coord) return;

            Selected = coord;
            Changed?.Invoke();
        }

        public void Clear()
        {
            if (!Selected.HasValue) return;

            Selected = null;
            Changed?.Invoke();
        }
    }
}
