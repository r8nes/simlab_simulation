using System;
using System.Collections.Generic;
using Godland.Core.World.Grid;
using Godland.Core.World.Timing;

namespace Godland.Game.Presentation.History
{
    public sealed class Chronicle
    {
        private readonly WorldClock _clock;
        private readonly List<ChronicleEntry> _entries = new();

        public Chronicle(WorldClock clock)
        {
            _clock = clock;
        }

        public event Action<ChronicleEntry> Added;

        public IReadOnlyList<ChronicleEntry> Entries => _entries;

        public void Add(string text, GridCoord? place = null)
        {
            var entry = new ChronicleEntry(_clock.Year, text, place);
            _entries.Add(entry);
            Added?.Invoke(entry);
        }
    }
}
