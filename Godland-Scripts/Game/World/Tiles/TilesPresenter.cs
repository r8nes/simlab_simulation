using System;
using System.Collections.Generic;
using Godland.Core.World.Grid;
using Godland.Core.World.Tiles;
using Godland.Game.World.Grid;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.World.Tiles
{
    public sealed class TilesPresenter : IStartable, IDisposable
    {
        private readonly WorldTiles _tiles;
        private readonly TileCatalog _catalog;
        private readonly GridGeometry _geometry;
        private readonly TilesView _view;
        private readonly IObjectResolver _resolver;
        private readonly TileAnimationConfig _animation;
        private readonly Dictionary<GridCoord, TileView> _views = new();

        public TilesPresenter(WorldTiles tiles, TileCatalog catalog, GridGeometry geometry, TilesView view, IObjectResolver resolver, TileAnimationConfig animation)
        {
            _tiles = tiles;
            _catalog = catalog;
            _geometry = geometry;
            _view = view;
            _resolver = resolver;
            _animation = animation;
        }

        public void Start()
        {
            foreach (var (coord, tileId) in _tiles.All)
                Show(coord, tileId);

            _tiles.Placed += Show;
            _tiles.Removed += Hide;
        }

        public void Dispose()
        {
            _tiles.Placed -= Show;
            _tiles.Removed -= Hide;
        }

        private void Hide(GridCoord coord)
        {
            if (_views.Remove(coord, out var view)) view.Disappear();
        }

        private void Show(GridCoord coord, string tileId)
        {
            var delay = 0f;
            if (_views.Remove(coord, out var previous))
            {
                previous.Disappear();
                delay = _animation.DisappearDuration;
            }

            var view = _resolver.Instantiate(_catalog.Get(tileId).Prefab, _view.Root);
            view.name = $"{tileId} {coord}";
            view.Place(_geometry.ToWorld(coord), _geometry.CellSize);
            view.Appear(delay);
            _views.Add(coord, view);
        }

        public bool TryGetView(GridCoord coord, out TileView view) => _views.TryGetValue(coord, out view);
    }
}
