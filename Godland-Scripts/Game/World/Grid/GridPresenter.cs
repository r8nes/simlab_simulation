using System;
using Godland.Core.World.Grid;
using Godland.Core.World.Tiles;
using VContainer.Unity;

namespace Godland.Game.World.Grid
{
    public sealed class GridPresenter : IInitializable, IDisposable
    {
        private readonly WorldGrid _grid;
        private readonly GridGeometry _geometry;
        private readonly GridView _view;
        private readonly WorldTiles _tiles;

        public GridPresenter(WorldGrid grid, GridGeometry geometry, GridView view, WorldTiles tiles)
        {
            _grid = grid;
            _geometry = geometry;
            _view = view;
            _tiles = tiles;
        }

        public void Initialize()
        {
            _view.Build(_grid.Width, _grid.Height, _geometry.Size);

            foreach (var coord in _grid.Coords)
                if (_grid.IsUnlocked(coord))
                    OnUnlocked(coord);

            _grid.Unlocked += OnUnlocked;
            _tiles.Placed += OnPlaced;
            _tiles.Removed += _view.SetUnlocked;
        }

        public void Dispose()
        {
            _grid.Unlocked -= OnUnlocked;
            _tiles.Placed -= OnPlaced;
            _tiles.Removed -= _view.SetUnlocked;
            _view.Release();
        }

        private void OnUnlocked(GridCoord coord)
        {
            if (!_tiles.TryGet(coord, out _)) _view.SetUnlocked(coord);

            foreach (var neighbor in _grid.Neighbors(coord))
                if (_grid.IsPurchasable(neighbor))
                    _view.SetPurchasable(neighbor);
        }

        private void OnPlaced(GridCoord coord, string _) => _view.SetOccupied(coord);
    }
}
