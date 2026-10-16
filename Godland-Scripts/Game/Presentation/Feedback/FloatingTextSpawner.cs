using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Godland.Core.World.Grid;
using Godland.Game.Presentation.Cameras;
using Godland.Game.World.Grid;
using Godland.Game.World.Tiles;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.Presentation.Feedback
{
    public sealed class FloatingTextSpawner : IDisposable
    {
        private readonly GridGeometry _geometry;
        private readonly CameraView _camera;
        private readonly TilesView _tilesView;
        private readonly FeedbackConfig _config;
        private readonly IObjectResolver _resolver;
        private readonly TileSelection _selection;
        private readonly PlayerFocus _focus;
        private readonly CancellationTokenSource _lifetime = new();
        private readonly Dictionary<GridCoord, CellQueue> _queues = new();

        public FloatingTextSpawner(
            GridGeometry geometry, CameraView camera, TilesView tilesView, FeedbackConfig config, IObjectResolver resolver, TileSelection selection,
            PlayerFocus focus)
        {
            _geometry = geometry;
            _camera = camera;
            _tilesView = tilesView;
            _config = config;
            _resolver = resolver;
            _selection = selection;
            _focus = focus;
        }

        public void Spawn(GridCoord coord, string text, FloatingTextStyle style)
        {
            if (_selection.Selected != coord && !_focus.Covers(coord)) return;

            if (!_queues.TryGetValue(coord, out var queue)) _queues[coord] = queue = new CellQueue();
            if (queue.Pending.Contains(text)) return;
            if (queue.Pending.Count >= _config.MaxPopupsPerCell && !IsImportant(style)) return;

            var now = Time.unscaledTime;
            var delay = Mathf.Max(0f, queue.NextTime - now);
            queue.NextTime = now + delay + _config.PopupInterval;
            queue.Pending.Add(text);
            ShowAsync(coord, text, style, delay, queue).Forget();
        }

        public void Dispose()
        {
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        private static bool IsImportant(FloatingTextStyle style) => style is FloatingTextStyle.Reward or FloatingTextStyle.Evolution;

        private async UniTaskVoid ShowAsync(GridCoord coord, string text, FloatingTextStyle style, float delay, CellQueue queue)
        {
            if (delay > 0f) await UniTask.Delay(TimeSpan.FromSeconds(delay), DelayType.UnscaledDeltaTime, cancellationToken: _lifetime.Token);
            queue.Pending.Remove(text);
            if (_selection.Selected != coord && !_focus.Covers(coord)) return;

            var (color, scale) = style switch
            {
                FloatingTextStyle.Reward => (_config.RewardColor, 1f),
                FloatingTextStyle.Evolution => (_config.EvolutionColor, 1f),
                FloatingTextStyle.Threat => (_config.ThreatColor, 0.8f),
                _ => (_config.FertilityColor, 0.6f)
            };

            var popup = _resolver.Instantiate(_config.FloatingTextPrefab, _tilesView.Root);
            var start = _geometry.ToWorld(coord) + Vector3.up * (_config.FloatingTextHeight * scale);
            await popup.PlayAsync(text, color, start, () => _camera.Rotation,
                _config.FloatingTextRise * scale, _config.FloatingTextDuration * Mathf.Max(scale, 0.7f), _lifetime.Token);
        }

        private sealed class CellQueue
        {
            public readonly List<string> Pending = new();
            public float NextTime;
        }
    }
}
