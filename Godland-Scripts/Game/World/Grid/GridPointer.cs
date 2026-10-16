using Godland.Core.World.Grid;
using Godland.Game.Presentation.Cameras;
using Godland.Game.Presentation.Inputs;
using UnityEngine;

namespace Godland.Game.World.Grid
{
    public sealed class GridPointer
    {
        private static readonly Plane GroundPlane = new(Vector3.up, Vector3.zero);

        private readonly PointerInput _pointer;
        private readonly CameraView _camera;
        private readonly WorldGrid _grid;
        private readonly GridGeometry _geometry;

        public GridPointer(PointerInput pointer, CameraView camera, WorldGrid grid, GridGeometry geometry)
        {
            _pointer = pointer;
            _camera = camera;
            _grid = grid;
            _geometry = geometry;
        }

        public GridCoord? HoveredCell => HoveredCoord is { } coord && _grid.IsUnlocked(coord) ? coord : null;

        public GridCoord? HoveredPurchasable => HoveredCoord is { } coord && _grid.IsPurchasable(coord) ? coord : null;

        private GridCoord? HoveredCoord
        {
            get
            {
                var ray = _camera.ScreenPointToRay(_pointer.Position);
                if (!GroundPlane.Raycast(ray, out var distance)) return null;
                return _geometry.TryGetCoord(ray.GetPoint(distance), out var coord) ? coord : null;
            }
        }
    }
}
