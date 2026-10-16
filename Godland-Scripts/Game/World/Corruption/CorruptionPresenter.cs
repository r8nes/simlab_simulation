using System;
using Godland.Core.World.Corruption;
using Godland.Core.World.Grid;
using Godland.Game.Presentation.Journal;
using VContainer.Unity;

namespace Godland.Game.World.Corruption
{
    public sealed class CorruptionPresenter : IStartable, IDisposable
    {
        private readonly CorruptionSpread _corruption;
        private readonly EventJournal _journal;

        public CorruptionPresenter(CorruptionSpread corruption, EventJournal journal)
        {
            _corruption = corruption;
            _journal = journal;
        }

        public void Start()
        {
            _corruption.Corrupted += OnCorrupted;
            _corruption.Healed += OnHealed;
        }

        public void Dispose()
        {
            _corruption.Corrupted -= OnCorrupted;
            _corruption.Healed -= OnHealed;
        }

        private void OnCorrupted(GridCoord _) => _journal.Write("Порча расползается: ещё одна земля осквернена", JournalTone.Threat);

        private void OnHealed(GridCoord _) => _journal.Write("Без Источника порча отступает: земля исцелилась", JournalTone.Good);
    }
}
