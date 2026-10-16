using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Godland.Game.Life.Features;
using Godland.Game.Life.People;
using Godland.Game.World.Tiles;
using Godland.Game.Powers.Titans;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.Presentation.Menu
{
    public sealed class MenuShowcasePresenter : IAsyncStartable, IDisposable
    {
        private readonly MenuSettings _settings;
        private readonly MenuShowcaseView _view;
        private readonly TileCatalog _tiles;
        private readonly FeatureCatalog _features;
        private readonly SettlementConfig _settlements;
        private readonly TitanConfig _titans;
        private readonly IObjectResolver _resolver;
        private readonly System.Random _random = new();
        private readonly List<TileView> _shown = new();

        public MenuShowcasePresenter(
            MenuSettings settings, MenuShowcaseView view, TileCatalog tiles, FeatureCatalog features, SettlementConfig settlements,
            TitanConfig titans, IObjectResolver resolver)
        {
            _settings = settings;
            _view = view;
            _tiles = tiles;
            _features = features;
            _settlements = settlements;
            _titans = titans;
            _resolver = resolver;
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            _view.Spin(_settings.TurnSeconds);
            while (!cancellation.IsCancellationRequested)
            {
                _view.ShowCaption(Compose());
                if (await UniTask.Delay(TimeSpan.FromSeconds(_settings.ShowSeconds), cancellationToken: cancellation).SuppressCancellationThrow()) return;
                Clear();
                _view.HideCaption();
                if (await UniTask.Delay(TimeSpan.FromSeconds(_settings.GapSeconds), cancellationToken: cancellation).SuppressCancellationThrow()) return;
            }
        }

        public void Dispose() => _view.StopSpin();

        private string Compose()
        {
            var parts = new List<string>();
            var tiles = _tiles.All.Where(candidate => candidate.Prefab != null).ToList();
            var tile = tiles[_random.Next(tiles.Count)];
            Show(tile.Prefab, _settings.TileSize, 0f);
            parts.Add(tile.DisplayName);

            var features = _features.All.Where(feature => feature.Prefab != null && _features.CanLiveOn(feature.Id, tile.Id)).ToList();
            if (features.Count > 0 && Roll(_settings.FeatureChancePercent))
            {
                var feature = features[_random.Next(features.Count)];
                Show(feature.Prefab, _settings.TileSize, 0.15f);
                parts.Add(feature.DisplayName);
            }

            if (_settlements.SettlementPrefab != null && _settlements.CanHost(tile.Id) && Roll(_settings.SettlementChancePercent))
                parts.Add(ShowSettlement());

            if (_titans.AvatarPrefab != null && _titans.All.Count > 0 && Roll(_settings.TitanChancePercent))
                parts.Add(ShowTitan());

            return string.Join(" · ", parts);
        }

        private string ShowSettlement()
        {
            var settlement = _resolver.Instantiate(_settlements.SettlementPrefab, _view.Anchor);
            var population = _random.Next(1, _settings.CityPopulation + 2);
            var village = population >= _settlements.VillagePopulation;
            var city = population >= _settings.CityPopulation;
            settlement.Body.Place(Vector3.zero, _settings.TileSize);
            settlement.SetPopulation(population, village, city);
            settlement.Body.Appear(0.3f);
            _shown.Add(settlement.Body);
            return city ? "город" : village ? "деревня" : "стоянка";
        }

        private string ShowTitan()
        {
            var titan = _titans.All[_random.Next(_titans.All.Count)];
            var avatar = _resolver.Instantiate(_titans.AvatarPrefab, _view.Anchor);
            avatar.Tint(titan.Color);
            avatar.Body.Place(Vector3.zero, _settings.TileSize * titan.Scale);
            avatar.Body.Appear(0.45f);
            _shown.Add(avatar.Body);
            return titan.DisplayName;
        }

        private void Show(TileView prefab, float size, float delay)
        {
            var view = _resolver.Instantiate(prefab, _view.Anchor);
            view.Place(Vector3.zero, size);
            view.Appear(delay);
            _shown.Add(view);
        }

        private void Clear()
        {
            foreach (var view in _shown.Where(view => view != null)) view.Disappear();
            _shown.Clear();
        }

        private bool Roll(int percent) => _random.Next(100) < percent;
    }
}
