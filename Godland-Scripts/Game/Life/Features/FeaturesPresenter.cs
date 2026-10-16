using System;
using System.Collections.Generic;
using Godland.Core.Life.Features;
using Godland.Core.World.Grid;
using Godland.Game.World.Grid;
using Godland.Game.World.Tiles;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.Life.Features
{
    public sealed class FeaturesPresenter : IStartable, IDisposable
    {
        private readonly TileFeatures _features;
        private readonly FeatureCatalog _catalog;
        private readonly GridGeometry _geometry;
        private readonly TilesView _root;
        private readonly IObjectResolver _resolver;
        private readonly TileAnimationConfig _animation;
        private readonly FeatureDepletion _depletion;
        private readonly Dictionary<GridCoord, TileView> _views = new();

        public FeaturesPresenter(
            TileFeatures features, FeatureCatalog catalog, GridGeometry geometry, TilesView root, IObjectResolver resolver, TileAnimationConfig animation,
            FeatureDepletion depletion)
        {
            _features = features;
            _catalog = catalog;
            _geometry = geometry;
            _root = root;
            _resolver = resolver;
            _animation = animation;
            _depletion = depletion;
        }

        public void Start()
        {
            _features.Added += Show;
            _features.Removed += Hide;
            _depletion.Depleted += OnDepleted;
            _depletion.Recovered += OnRecovered;
        }

        public void Dispose()
        {
            _features.Added -= Show;
            _features.Removed -= Hide;
            _depletion.Depleted -= OnDepleted;
            _depletion.Recovered -= OnRecovered;
        }

        public bool TryGetView(GridCoord coord, out TileView view) => _views.TryGetValue(coord, out view);

        private void Show(GridCoord coord, string featureId)
        {
            var view = _resolver.Instantiate(_catalog.Get(featureId).Prefab, _root.Root);
            view.name = $"{featureId} {coord}";
            view.Place(_geometry.ToWorld(coord), _geometry.CellSize);
            if (_features.IsWild(coord)) view.Shrink(_animation.WildScale);
            view.Appear(_views.ContainsKey(coord) ? _animation.DisappearDuration : 0f);
            _views[coord] = view;
        }

        private void OnDepleted(GridCoord coord)
        {
            if (_views.TryGetValue(coord, out var view)) view.SetDepleted(true);
        }

        private void OnRecovered(GridCoord coord)
        {
            if (_views.TryGetValue(coord, out var view)) view.SetDepleted(false);
        }

        private void Hide(GridCoord coord, string _)
        {
            if (_views.Remove(coord, out var view)) view.Disappear();
        }
    }
}
