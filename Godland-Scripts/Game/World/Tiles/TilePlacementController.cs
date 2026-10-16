using Godland.Core.World.Creation;
using Godland.Core.World.Grid;
using Godland.Core.World.Tiles;
using Godland.Game.Presentation.Cameras;
using Godland.Game.World.Grid;
using Godland.Game.Presentation.Inputs;
using UnityEngine;
using VContainer.Unity;

namespace Godland.Game.World.Tiles
{
    public sealed class TilePlacementController : ITickable
    {
        private readonly PointerInput _input;
        private readonly GridPointer _pointer;
        private readonly WorldTiles _tiles;
        private readonly TilePlacement _placement;
        private readonly CellPurchase _purchase;
        private readonly TileSelection _selection;
        private readonly CameraController _camera;
        private readonly CameraConfig _cameraConfig;
        private readonly GridGeometry _geometry;
        private readonly Godland.Game.Writer.Commands.CommandTargeting _targeting;

        private GridCoord? _lastClicked;
        private float _lastClickTime = float.NegativeInfinity;

        public TilePlacementController(
            PointerInput input, GridPointer pointer, WorldTiles tiles, TilePlacement placement, CellPurchase purchase, TileSelection selection,
            CameraController camera, CameraConfig cameraConfig, GridGeometry geometry, Godland.Game.Writer.Commands.CommandTargeting targeting)
        {
            _targeting = targeting;
            _input = input;
            _pointer = pointer;
            _tiles = tiles;
            _placement = placement;
            _purchase = purchase;
            _selection = selection;
            _camera = camera;
            _cameraConfig = cameraConfig;
            _geometry = geometry;
        }

        public void Tick()
        {
            if (_targeting.IsActive)
            {
                if (_input.CancelPressed) _targeting.Cancel();
                else if (_input.PrimaryClicked && _pointer.HoveredCell is { } target) _targeting.TryPick(target);
                return;
            }

            if (_input.CancelPressed) _selection.Clear();
            if (!_input.PrimaryClicked) return;

            if (_pointer.HoveredCell is not { } coord)
            {
                _selection.Clear();
                if (_pointer.HoveredPurchasable is { } edge) _purchase.TryBuy(edge);
                return;
            }

            if (_tiles.TryGet(coord, out _))
            {
                if (IsDoubleClick(coord)) _camera.FocusOn(_geometry.ToWorld(coord));
                _selection.Select(coord);
                return;
            }

            _selection.Clear();
            _placement.TryPlace(coord);
        }

        private bool IsDoubleClick(GridCoord coord)
        {
            var now = Time.unscaledTime;
            var isDouble = _lastClicked == coord && now - _lastClickTime <= _cameraConfig.DoubleClickInterval;
            _lastClicked = isDouble ? null : coord;
            _lastClickTime = now;
            return isDouble;
        }
    }
}
