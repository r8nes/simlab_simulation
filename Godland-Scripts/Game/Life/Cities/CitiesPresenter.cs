using System;
using Godland.Core.Life.Cities;
using Godland.Core.World.Grid;
using Godland.Game.Presentation.Journal;
using Godland.Game.World.Tiles;
using VContainer.Unity;

namespace Godland.Game.Life.Cities
{
    public sealed class CitiesPresenter : IStartable, IDisposable
    {
        private readonly Urbanization _cities;
        private readonly EventJournal _journal;
        private readonly TileCatalog _catalog;

        public CitiesPresenter(Urbanization cities, EventJournal journal, TileCatalog catalog)
        {
            _cities = cities;
            _journal = journal;
            _catalog = catalog;
        }

        public void Start()
        {
            _cities.Risen += OnRisen;
            _cities.Fallen += OnFallen;
            _cities.Built += OnBuilt;
        }

        public void Dispose()
        {
            _cities.Risen -= OnRisen;
            _cities.Fallen -= OnFallen;
            _cities.Built -= OnBuilt;
        }

        private void OnRisen(GridCoord _) => _journal.Write("Деревня разрослась в город", JournalTone.Good);

        private void OnFallen(GridCoord _) => _journal.Write("Город пришёл в упадок и снова стал деревней", JournalTone.Threat);

        private void OnBuilt(GridCoord city, GridCoord cell, string tileId) =>
            _journal.Write($"Город застроил окрестности: {_catalog.Get(tileId).DisplayName}", JournalTone.Info);
    }
}
