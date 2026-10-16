using System;
using System.Collections.Generic;
using Godland.Core.Life.Cities;
using Godland.Core.World.Grid;
using Godland.Core.Life.People;
using Godland.Game.Life.Features;
using Godland.Game.Presentation.Feedback;
using Godland.Game.World.Grid;
using Godland.Game.World.Tiles;
using VContainer;
using VContainer.Unity;
using Godland.Game.Presentation.Journal;

namespace Godland.Game.Life.People
{
    public sealed class SettlementsPresenter : IStartable, IDisposable
    {
        private readonly Settlements _settlements;
        private readonly SettlementConfig _config;
        private readonly TileCatalog _tiles;
        private readonly FeatureCatalog _features;
        private readonly GridGeometry _geometry;
        private readonly TilesView _root;
        private readonly IObjectResolver _resolver;
        private readonly FloatingTextSpawner _spawner;
        private readonly EventJournal _journal;
        private readonly Urbanization _cities;
        private readonly Dictionary<GridCoord, SettlementView> _views = new();

        public SettlementsPresenter(
            Settlements settlements, SettlementConfig config, TileCatalog tiles, FeatureCatalog features,
            GridGeometry geometry, TilesView root, IObjectResolver resolver, FloatingTextSpawner spawner,
            EventJournal journal, Urbanization cities)
        {
            _settlements = settlements;
            _config = config;
            _tiles = tiles;
            _features = features;
            _geometry = geometry;
            _root = root;
            _resolver = resolver;
            _spawner = spawner;
            _journal = journal;
            _cities = cities;
        }

        public void Start()
        {
            _settlements.Founded += Show;
            _settlements.Abandoned += Hide;
            _settlements.PopulationChanged += OnPopulationChanged;
            _settlements.CultureChanged += OnCultureChanged;
            _settlements.Migrated += OnMigrated;
            _settlements.HuntStarted += OnHuntStarted;
            _settlements.HuntFinished += OnHuntFinished;
            _settlements.WorldShaped += OnWorldShaped;
            _cities.Risen += Refresh;
            _cities.Fallen += Refresh;
        }

        public void Dispose()
        {
            _settlements.Founded -= Show;
            _settlements.Abandoned -= Hide;
            _settlements.PopulationChanged -= OnPopulationChanged;
            _settlements.CultureChanged -= OnCultureChanged;
            _settlements.Migrated -= OnMigrated;
            _settlements.HuntStarted -= OnHuntStarted;
            _settlements.HuntFinished -= OnHuntFinished;
            _settlements.WorldShaped -= OnWorldShaped;
            _cities.Risen -= Refresh;
            _cities.Fallen -= Refresh;
        }

        private void Refresh(GridCoord coord)
        {
            if (_views.TryGetValue(coord, out var view) && _settlements.TryGet(coord, out var population))
                view.SetPopulation(population, population >= _config.VillagePopulation, _cities.IsCity(coord));
        }

        private void Show(GridCoord coord)
        {
            var view = _resolver.Instantiate(_config.SettlementPrefab, _root.Root);
            view.name = $"Settlement {coord}";
            view.Body.Place(_geometry.ToWorld(coord), _geometry.CellSize);
            view.Body.Appear(0f);
            _views[coord] = view;
            var people = _config.PeopleName(_settlements.CultureOf(coord));
            _spawner.Spawn(coord, $"Появились: {people}", FloatingTextStyle.Evolution);
            if (_settlements.All.Count == 1) _journal.Write($"Появились первые жители: {people}", JournalTone.Good);
        }

        private void Hide(GridCoord coord)
        {
            if (!_views.Remove(coord, out var view)) return;

            view.Body.Disappear();
            _journal.Write("Поселение опустело", JournalTone.Threat);
            _spawner.Spawn(coord, "Жители ушли", FloatingTextStyle.Threat);
        }

        private void OnPopulationChanged(GridCoord coord, int previous, int population)
        {
            if (!_views.TryGetValue(coord, out var view)) return;

            var village = population >= _config.VillagePopulation;
            view.SetPopulation(population, village, _cities.IsCity(coord));

            if (village && previous < _config.VillagePopulation && previous > 0)
            {
                _spawner.Spawn(coord, "Стоянка стала деревней!", FloatingTextStyle.Evolution);
                _journal.Write($"{_config.PeopleName(_settlements.CultureOf(coord))}: стоянка выросла в деревню", JournalTone.Good);
            }
            else if (previous > 0 && population > previous)
                _spawner.Spawn(coord, $"+{population - previous} житель", FloatingTextStyle.Fertility);
        }

        private void OnCultureChanged(GridCoord coord, SettlementCulture culture) =>
            _spawner.Spawn(coord, $"Жители стали: {_config.CultureById(culture.Id).DisplayName}", FloatingTextStyle.Evolution);

        private void OnMigrated(GridCoord from, GridCoord to) =>
            _spawner.Spawn(to, "Переселенцы!", FloatingTextStyle.Evolution);

        private void OnHuntStarted(GridCoord settlement, SettlementHunt hunt) =>
            _spawner.Spawn(hunt.Prey, $"! Жители охотятся на: {_features.Get(hunt.PreyFeatureId).DisplayName}", FloatingTextStyle.Threat);

        private void OnHuntFinished(GridCoord settlement, SettlementHunt hunt) =>
            _spawner.Spawn(settlement, $"Жители добыли: {_features.Get(hunt.PreyFeatureId).DisplayName} (восстановится через {_features.RecoveryYears} лет)", FloatingTextStyle.Reward);

        private void OnWorldShaped(GridCoord settlement, GridCoord target, string tileId) =>
            _spawner.Spawn(target, $"Жители создали: {_tiles.Get(tileId).DisplayName}", FloatingTextStyle.Evolution);
    }
}
