using System;
using Godland.Core.World.Grid;
using Godland.Game.Presentation.Journal;
using VContainer.Unity;
using Godland.Core.World.Climate;

namespace Godland.Game.World.Climate
{
    public sealed class ClimatePresenter : IStartable, IDisposable
    {
        private readonly WorldClimate _climate;
        private readonly EventJournal _journal;

        public ClimatePresenter(WorldClimate climate, EventJournal journal)
        {
            _climate = climate;
            _journal = journal;
        }

        public void Start()
        {
            _climate.Desertified += OnDesertified;
            _climate.Frozen += OnFrozen;
            _climate.ForestGrew += OnForestGrew;
        }

        public void Dispose()
        {
            _climate.Desertified -= OnDesertified;
            _climate.Frozen -= OnFrozen;
            _climate.ForestGrew -= OnForestGrew;
        }

        private void OnDesertified(GridCoord _) => _journal.Write("Иссохшая земля стала пустыней", JournalTone.Threat);

        private void OnForestGrew(GridCoord _) => _journal.Write("Лес разросся на соседнюю землю", JournalTone.Info);

        private void OnFrozen(GridCoord _, GridCoord __) => _journal.Write("Ледник сполз с высокогорья: земля под снегом", JournalTone.Info);
    }
}
