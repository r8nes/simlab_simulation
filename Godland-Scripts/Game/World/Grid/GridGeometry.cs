using Godland.Core.World.Grid;
using Godland.Game.Presentation.Cameras;
using UnityEngine;

namespace Godland.Game.World.Grid
{
    public sealed class GridGeometry : ICameraBounds
    {
        private readonly WorldGrid _grid;
        private readonly GridConfig _config;

        public GridGeometry(WorldGrid grid, GridConfig config)
        {
            _grid = grid;
            _config = config;
        }

        public Vector2 Size => new Vector2(_grid.Width, _grid.Height) * _config.CellSize;

        public float CellSize => _config.CellSize;

        public Rect FocusBounds => new(-Size * 0.5f, Size);

        public Vector3 ToWorld(GridCoord coord)
        {
            var bounds = FocusBounds;
            return new Vector3(
                bounds.xMin + (coord.X + 0.5f) * _config.CellSize,
                0f,
                bounds.yMin + (coord.Y + 0.5f) * _config.CellSize);
        }

        public bool TryGetCoord(Vector3 point, out GridCoord coord)
        {
            var bounds = FocusBounds;
            coord = new GridCoord(
                Mathf.FloorToInt((point.x - bounds.xMin) / _config.CellSize),
                Mathf.FloorToInt((point.z - bounds.yMin) / _config.CellSize));
            return _grid.Contains(coord);
        }
    }
}
