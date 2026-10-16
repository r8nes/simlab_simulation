using System;
using Godland.Core.Life.Cities;
using Godland.Core.Powers.Gods;
using Godland.Core.World.Grid;
using Godland.Core.Life.Heroes;
using Godland.Core.Life.Kingdoms;
using Godland.Core.Life.People;
using Godland.Core.World.Tiles;
using Godland.Game.Powers.Gods;
using VContainer.Unity;

namespace Godland.Game.Presentation.History
{
    public sealed class TileNotesPresenter : IStartable, IDisposable
    {
        private readonly TileHistory _history;
        private readonly Settlements _settlements;
        private readonly Pantheon _pantheon;
        private readonly Temples _temples;
        private readonly Urbanization _cities;
        private readonly Realms _realms;
        private readonly HeroSaga _heroes;
        private readonly GodsConfig _gods;

        public TileNotesPresenter(
            TileHistory history, Settlements settlements, Pantheon pantheon, Temples temples, Urbanization cities, Realms realms,
            HeroSaga heroes, GodsConfig gods)
        {
            _history = history;
            _settlements = settlements;
            _pantheon = pantheon;
            _temples = temples;
            _cities = cities;
            _realms = realms;
            _heroes = heroes;
            _gods = gods;
        }

        public void Start()
        {
            _settlements.Founded += OnFounded;
            _settlements.Abandoned += OnAbandoned;
            _pantheon.Born += OnGodBorn;
            _pantheon.Fallen += OnGodFallen;
            _pantheon.Died += OnGodDied;
            _temples.Built += OnTempleBuilt;
            _cities.Risen += OnCityRisen;
            _realms.Founded += OnKingdomFounded;
            _heroes.Born += OnHeroBorn;
        }

        public void Dispose()
        {
            _settlements.Founded -= OnFounded;
            _settlements.Abandoned -= OnAbandoned;
            _pantheon.Born -= OnGodBorn;
            _pantheon.Fallen -= OnGodFallen;
            _pantheon.Died -= OnGodDied;
            _temples.Built -= OnTempleBuilt;
            _cities.Risen -= OnCityRisen;
            _realms.Founded -= OnKingdomFounded;
            _heroes.Born -= OnHeroBorn;
        }

        private void OnFounded(GridCoord coord) => _history.Note(coord, "появились жители");

        private void OnAbandoned(GridCoord coord) => _history.Note(coord, "жители ушли");

        private void OnGodBorn(God god) => _history.Note(god.Seat, $"здесь престол бога: {_gods.NameOf(god)}");

        private void OnGodFallen(God god) => _history.Note(god.Seat, $"бог пал: {_gods.NameOf(god)}");

        private void OnGodDied(God god) => _history.Note(god.Seat, $"бог умер от старости: {_gods.NameOf(god)}");

        private void OnTempleBuilt(God god, GridCoord cell) => _history.Note(cell, $"воздвигнут храм: {_gods.NameOf(god)}");

        private void OnCityRisen(GridCoord coord) => _history.Note(coord, "деревня стала городом");

        private void OnKingdomFounded(Kingdom kingdom) => _history.Note(kingdom.Capital, $"столица царства {kingdom.Name}");

        private void OnHeroBorn(Hero hero) => _history.Note(hero.Home, $"здесь родился герой {hero.Name}");
    }
}
