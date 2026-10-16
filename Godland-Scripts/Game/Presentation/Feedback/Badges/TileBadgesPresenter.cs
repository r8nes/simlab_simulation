using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.Life.Features;
using Godland.Core.World.Grid;
using Godland.Core.World.Tiles;
using Godland.Core.World.Timing;
using Godland.Game.Presentation.Cameras;
using Godland.Game.World.Grid;
using Godland.Game.World.Tiles;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.Presentation.Feedback.Badges
{
    public sealed class TileBadgesPresenter : IStartable, ITickable, IDisposable
    {
        private readonly IReadOnlyList<ITileBadgeSource> _sources;
        private readonly WorldTiles _tiles;
        private readonly TileFeatures _features;
        private readonly WorldClock _clock;
        private readonly FeedbackConfig _config;
        private readonly GridGeometry _geometry;
        private readonly TilesView _root;
        private readonly CameraView _camera;
        private readonly IObjectResolver _resolver;
        private readonly Dictionary<GridCoord, TileBadgesView> _views = new();
        private readonly Dictionary<GridCoord, List<BadgeKind>> _shown = new();
        private readonly List<BadgeKind> _buffer = new();

        public TileBadgesPresenter(
            IEnumerable<ITileBadgeSource> sources, WorldTiles tiles, TileFeatures features, WorldClock clock, FeedbackConfig config,
            GridGeometry geometry, TilesView root, CameraView camera, IObjectResolver resolver)
        {
            _sources = sources.ToList();
            _tiles = tiles;
            _features = features;
            _clock = clock;
            _config = config;
            _geometry = geometry;
            _root = root;
            _camera = camera;
            _resolver = resolver;
        }

        public void Start()
        {
            _clock.Ticked += OnTicked;
            _tiles.Placed += OnCellChanged;
            _tiles.Removed += OnCellRemoved;
            _features.Added += OnCellChanged;
            _features.Removed += OnCellChanged;
        }

        public void Dispose()
        {
            _clock.Ticked -= OnTicked;
            _tiles.Placed -= OnCellChanged;
            _tiles.Removed -= OnCellRemoved;
            _features.Added -= OnCellChanged;
            _features.Removed -= OnCellChanged;
        }

        public void Tick()
        {
            var rotation = _camera.Rotation;
            foreach (var view in _views.Values)
                if (view.gameObject.activeSelf)
                    view.Face(rotation);
        }

        private void OnTicked(int _) => RefreshAll();

        private void OnCellChanged(GridCoord _, string __) => RefreshAll();

        private void OnCellRemoved(GridCoord _) => RefreshAll();

        private void RefreshAll()
        {
            foreach (var coord in _tiles.All.Keys.Concat(_views.Keys).Distinct().ToList())
                Refresh(coord);
        }

        private void Refresh(GridCoord coord)
        {
            _buffer.Clear();
            if (_tiles.TryGet(coord, out _))
                foreach (var source in _sources)
                    source.Collect(coord, _buffer);
            _buffer.Sort();

            if (_shown.TryGetValue(coord, out var shown) && shown.SequenceEqual(_buffer)) return;
            _shown[coord] = new List<BadgeKind>(_buffer);

            if (_buffer.Count == 0)
            {
                if (_views.TryGetValue(coord, out var hidden)) hidden.Hide();
                return;
            }

            var position = _geometry.ToWorld(coord) + Vector3.up * (_config.BadgeHeight * _geometry.CellSize);
            ViewFor(coord).Show(position, _buffer.Select(_config.BadgeFor).ToList());
        }

        private TileBadgesView ViewFor(GridCoord coord)
        {
            if (_views.TryGetValue(coord, out var view)) return view;

            view = _resolver.Instantiate(_config.BadgesPrefab, _root.Root);
            view.name = $"Badges {coord}";
            _views[coord] = view;
            return view;
        }
    }
}
