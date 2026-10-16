using System;
using Godland.Core.Powers.Gods;
using Godland.Core.World.Grid;
using Godland.Game.Presentation.Journal;
using VContainer.Unity;

namespace Godland.Game.Powers.Gods
{
    public sealed class TemplesPresenter : IStartable, IDisposable
    {
        private readonly Temples _temples;
        private readonly Pantheon _pantheon;
        private readonly GodsConfig _config;
        private readonly EventJournal _journal;

        public TemplesPresenter(Temples temples, Pantheon pantheon, GodsConfig config, EventJournal journal)
        {
            _temples = temples;
            _pantheon = pantheon;
            _config = config;
            _journal = journal;
        }

        public void Start()
        {
            _temples.Built += OnBuilt;
            _temples.Lost += OnLost;
            _temples.Desecrated += OnDesecrated;
            _pantheon.Forgotten += OnForgotten;
        }

        public void Dispose()
        {
            _temples.Built -= OnBuilt;
            _temples.Lost -= OnLost;
            _temples.Desecrated -= OnDesecrated;
            _pantheon.Forgotten -= OnForgotten;
        }

        private string NameOf(God god) => _config.NameOf(god);

        private void OnBuilt(God god, GridCoord _) =>
            _journal.Write($"Город воздвиг храм: {NameOf(god)} (храмов: {_temples.CountOf(god)})", JournalTone.Info);

        private void OnLost(God god, GridCoord _) => _journal.Write($"Храм заброшен: {NameOf(god)}", JournalTone.Info);

        private void OnDesecrated(God god, GridCoord _) => _journal.Write($"Храм осквернён: {NameOf(god)} слабеет", JournalTone.Threat);

        private void OnForgotten(God god) => _journal.Write($"Без храмов бог забыт: {NameOf(god)}", JournalTone.Threat);
    }
}
