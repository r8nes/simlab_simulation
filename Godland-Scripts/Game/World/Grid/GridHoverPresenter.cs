using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Creation;
using Godland.Core.World.Grid;
using Godland.Core.World.Tiles;
using Godland.Game.World.Tiles;
using UnityEngine;
using VContainer.Unity;

namespace Godland.Game.World.Grid
{
    public sealed class GridHoverPresenter : ITickable
    {
        private const float BuildableMark = 0.25f;
        private const float RefreshSeconds = 0.25f;

        private readonly GridPointer _pointer;
        private readonly TilePlacement _placement;
        private readonly CellPurchase _purchase;
        private readonly GridConfig _config;
        private readonly GridView _view;
        private readonly WorldGrid _grid;
        private readonly WorldTiles _tiles;
        private readonly Dictionary<GridCoord, float> _written = new();

        private float _visibility;
        private float _nextRefresh;

        public GridHoverPresenter(
            GridPointer pointer, TilePlacement placement, CellPurchase purchase, GridConfig config, GridView view, WorldGrid grid, WorldTiles tiles)
        {
            _pointer = pointer;
            _placement = placement;
            _purchase = purchase;
            _config = config;
            _view = view;
            _grid = grid;
            _tiles = tiles;
        }

        public void Tick()
        {
            var hovered = _pointer.HoveredCell ?? _pointer.HoveredPurchasable;
            if (Time.unscaledTime >= _nextRefresh || hovered != _lastHovered)
            {
                _nextRefresh = Time.unscaledTime + RefreshSeconds;
                _lastHovered = hovered;
                Refresh(hovered);
            }

            UpdateVisibility(hovered.HasValue ? 1f : _config.IdleVisibility);
        }

        private GridCoord? _lastHovered;

        private void Refresh(GridCoord? hovered)
        {
            var wanted = new Dictionary<GridCoord, float>();
            foreach (var cell in _grid.Coords.Concat(_grid.Outside).Where(_grid.IsUnlocked))
                if (_placement.CanPlace(cell)) wanted[cell] = BuildableMark;

            if (hovered is { } target)
                wanted[target] = _tiles.TryGet(target, out _) || CanAct(target) ? 1f : 0.05f;

            foreach (var cell in _written.Keys.Where(cell => !wanted.ContainsKey(cell)).ToList())
            {
                _view.SetHighlight(cell, 0f);
                _written.Remove(cell);
            }

            foreach (var (cell, value) in wanted)
            {
                if (_written.TryGetValue(cell, out var current) && Mathf.Approximately(current, value)) continue;

                _view.SetHighlight(cell, value);
                _written[cell] = value;
            }
        }

        private bool CanAct(GridCoord coord) => _placement.CanPlace(coord) || _purchase.CanBuy(coord);

        private void UpdateVisibility(float target)
        {
            if (Mathf.Approximately(_visibility, target)) return;

            _visibility = Mathf.MoveTowards(_visibility, target, _config.RevealSpeed * Time.unscaledDeltaTime);
            _view.SetVisibility(_visibility);
        }
    }
}
