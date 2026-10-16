using System;
using Godland.Core.World.Grid;
using Godland.Core.World.Tiles;
using Godland.Game.Presentation.History;
using Godland.Game.Presentation.Journal;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.Writer.Threads
{
    public sealed class WriterPresenter : IStartable, IDisposable
    {
        private readonly WriterThreads _threads;
        private readonly EventJournal _journal;
        private readonly Chronicle _chronicle;
        private readonly TileHistory _history;

        public WriterPresenter(WriterThreads threads, EventJournal journal, Chronicle chronicle, TileHistory history)
        {
            _threads = threads;
            _journal = journal;
            _chronicle = chronicle;
            _history = history;
        }

        public void Start() => _threads.Happened += OnHappened;

        public void Dispose() => _threads.Happened -= OnHappened;

        private void OnHappened(string text, GridCoord? place)
        {
            _journal.Write(text, JournalTone.Info);
            _chronicle.Add(text, place);
            if (place is { } cell) _history.Note(cell, text);
        }
    }

    public sealed class WriterInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<WriterPresenter>();
        }
    }
}
