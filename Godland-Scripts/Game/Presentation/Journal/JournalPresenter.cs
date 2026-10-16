using System;
using VContainer.Unity;

namespace Godland.Game.Presentation.Journal
{
    public sealed class JournalPresenter : IStartable, IDisposable
    {
        private readonly EventJournal _journal;
        private readonly JournalView _view;

        public JournalPresenter(EventJournal journal, JournalView view)
        {
            _journal = journal;
            _view = view;
        }

        public void Start()
        {
            _view.Show(_journal.Entries);
            _journal.Changed += Refresh;
        }

        public void Dispose() => _journal.Changed -= Refresh;

        private void Refresh() => _view.Show(_journal.Entries);
    }
}
