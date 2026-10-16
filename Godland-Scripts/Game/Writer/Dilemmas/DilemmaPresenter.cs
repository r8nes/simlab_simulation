using System;
using System.Collections.Generic;
using Godland.Game.Presentation.History;
using Godland.Game.Presentation.Journal;
using Godland.Game.Presentation.Timing;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.Writer.Dilemmas
{
    public sealed class DilemmaPresenter : IStartable, IDisposable
    {
        private readonly DilemmaQueue _queue;
        private readonly DilemmaView _view;
        private readonly TimeFlow _time;
        private readonly EventJournal _journal;
        private readonly Chronicle _chronicle;
        private bool _blocking;

        public DilemmaPresenter(DilemmaQueue queue, DilemmaView view, TimeFlow time, EventJournal journal, Chronicle chronicle)
        {
            _queue = queue;
            _view = view;
            _time = time;
            _journal = journal;
            _chronicle = chronicle;
        }

        public void Start()
        {
            _view.Hide();
            _queue.Offered += OnOffered;
            _queue.Resolved += OnResolved;
        }

        public void Dispose()
        {
            _queue.Offered -= OnOffered;
            _queue.Resolved -= OnResolved;
        }

        private void OnOffered(Dilemma dilemma)
        {
            if (!_blocking)
            {
                _time.Block();
                _blocking = true;
            }

            _view.Show(dilemma.Title, dilemma.Text);
            foreach (var option in dilemma.Options)
            {
                var label = $"{option.Label}{Cost(option)}";
                _view.AddOption(_queue.CanAfford(option) ? label : $"<color=#888888>{label}</color>", _queue.CanAfford(option) ? () => Choose(option) : null);
            }
        }

        private void Choose(DilemmaOption option)
        {
            _queue.Choose(option);
            if (_queue.Current != null) return;

            _view.Hide();
            if (!_blocking) return;

            _time.Unblock();
            _blocking = false;
        }

        private void OnResolved(Dilemma dilemma, DilemmaOption option)
        {
            var text = $"{dilemma.Title}: писатель выбрал «{option.Label}»";
            _journal.Write(text, JournalTone.Good);
            _chronicle.Add(text, dilemma.Place);
        }

        private static string Cost(DilemmaOption option)
        {
            var parts = new List<string>();
            if (option.Creation > 0) parts.Add($"{option.Creation} созид.");
            if (option.Faith > 0) parts.Add($"{option.Faith} веры");
            return parts.Count == 0 ? string.Empty : $"  <size=80%>· {string.Join(", ", parts)}</size>";
        }
    }

    public sealed class DilemmasInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<DilemmaView>();
            builder.RegisterEntryPoint<DilemmaPresenter>();
        }
    }
}
