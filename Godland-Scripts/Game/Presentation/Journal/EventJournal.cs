using System;
using System.Collections.Generic;
using Godland.Core.World.Timing;

namespace Godland.Game.Presentation.Journal
{
    public sealed class EventJournal
    {
        private const int Capacity = 50;

        private readonly WorldClock _clock;
        private readonly List<JournalEntry> _entries = new();

        public EventJournal(WorldClock clock)
        {
            _clock = clock;
        }

        public event Action Changed;

        public IReadOnlyList<JournalEntry> Entries => _entries;

        public void Write(string text, JournalTone tone = JournalTone.Info)
        {
            if (_entries.Count > 0 && _entries[^1].Year == _clock.Year && _entries[^1].Text == text)
            {
                _entries[^1] = new JournalEntry(_clock.Year, text, tone, _entries[^1].Repeats + 1);
                Changed?.Invoke();
                return;
            }

            _entries.Add(new JournalEntry(_clock.Year, text, tone));
            if (_entries.Count > Capacity) _entries.RemoveAt(0);
            Changed?.Invoke();
        }
    }
}
