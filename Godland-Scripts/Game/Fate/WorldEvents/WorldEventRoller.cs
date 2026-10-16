using System;
using System.Linq;
using Godland.Core;
using Godland.Core.Fate.Epochs;
using Godland.Game.Presentation.Journal;

namespace Godland.Game.Fate.WorldEvents
{
    public sealed class WorldEventRoller : IWorldStep
    {
        private readonly Godland.Core.Flow.FlowBias _flow;
        private readonly WorldEventsConfig _config;
        private readonly WorldEventContext _context;
        private readonly WorldEpoch _epoch;
        private readonly EventJournal _journal;
        private readonly System.Collections.Generic.HashSet<WorldEventDefinition> _happened = new();

        public WorldEventRoller(WorldEventsConfig config, WorldEventContext context, WorldEpoch epoch, EventJournal journal, Godland.Core.Flow.FlowBias flow)
        {
            _flow = flow;
            _config = config;
            _context = context;
            _epoch = epoch;
            _journal = journal;
        }

        public event Action<WorldEventDefinition> Happened;

        public void Advance(int year)
        {
            if (year % _config.Period != 0 || _context.Random.Next(100) >= _config.ChancePercent) return;

            var candidates = _config.Events.Where(worldEvent => worldEvent.FitsEpoch(_epoch.Index) && (worldEvent.Repeatable || !_happened.Contains(worldEvent)) && _context.Random.Next(100) < _flow.Chance($"event:{worldEvent.name}", worldEvent.ChancePercent) && worldEvent.CanHappen(_context)).ToList();
            var total = candidates.Sum(worldEvent => _flow.Weight($"event:{worldEvent.name}", worldEvent.Weight));
            if (total <= 0) return;

            var roll = _context.Random.Next(total);
            var chosen = candidates.First(worldEvent => (roll -= _flow.Weight($"event:{worldEvent.name}", worldEvent.Weight)) < 0);
            _flow.Note($"event:{chosen.name}");
            _context.Outcome = null;
            chosen.Apply(_context);
            _happened.Add(chosen);
            _journal.Write($"{chosen.Title}: {_context.Outcome ?? chosen.Text}", chosen.Tone);
            Happened?.Invoke(chosen);
        }
    }
}
