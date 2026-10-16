using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core;
using Godland.Core.Fate.Epochs;
using Godland.Core.Life.Heroes;

namespace Godland.Game.Powers.Prophecies
{
    public sealed class ProphecyKeeper : IWorldStep, IDisposable
    {
        private const int RetryPeriod = 100;

        private readonly ProphecyConfig _config;
        private readonly ProphecyContext _context;
        private readonly WorldEpoch _epoch;
        private readonly HeroSaga _heroes;
        private readonly List<MilestoneDefinition> _milestones = new();
        private readonly Queue<ProphecyDefinition> _pending = new();
        private readonly List<ProphecyDefinition> _postponed = new();

        public ProphecyKeeper(ProphecyConfig config, ProphecyContext context, WorldEpoch epoch, HeroSaga heroes)
        {
            _config = config;
            _context = context;
            _epoch = epoch;
            _heroes = heroes;

            _epoch.Advanced += OnEpochAdvanced;
            _heroes.Triumphed += OnHeroTriumphed;
            _context.Settlements.Abandoned += OnAbandoned;
        }

        public event Action<ProphecyDefinition, int> Announced;
        public event Action<ProphecyDefinition, MilestoneDefinition, bool> Resolved;

        public ProphecyDefinition Active { get; private set; }
        public int Deadline { get; private set; }
        public IReadOnlyList<MilestoneDefinition> Milestones => _milestones;

        public void Dispose()
        {
            _epoch.Advanced -= OnEpochAdvanced;
            _heroes.Triumphed -= OnHeroTriumphed;
            _context.Settlements.Abandoned -= OnAbandoned;
        }

        public void Advance(int year)
        {
            if (Active == null && _pending.Count == 0 && _postponed.Count > 0 && year % RetryPeriod == 0)
            {
                foreach (var prophecy in _postponed) _pending.Enqueue(prophecy);
                _postponed.Clear();
            }

            while (Active == null && _pending.Count > 0) TryAnnounce(_pending.Dequeue(), year);
            if (Active == null) return;

            if (Active.IsMet(_context)) Resolve(true);
            else if (year >= Deadline) Resolve(false);
        }

        private void TryAnnounce(ProphecyDefinition prophecy, int year)
        {
            _context.Record.Reset(year);
            if (prophecy.IsMet(_context))
            {
                _postponed.Add(prophecy);
                return;
            }

            Active = prophecy;
            Deadline = year + prophecy.DurationYears;
            _context.Record.Reset(year);
            Announced?.Invoke(prophecy, Deadline);
        }

        private void Resolve(bool fulfilled)
        {
            var prophecy = Active;
            var milestone = fulfilled ? prophecy.Fulfilled : prophecy.Prevented;
            Active = null;
            milestone.Apply(_context);
            _milestones.Add(milestone);
            Resolved?.Invoke(prophecy, milestone, fulfilled);
        }

        private void OnEpochAdvanced(int index)
        {
            foreach (var prophecy in _postponed) _pending.Enqueue(prophecy);
            _postponed.Clear();
            foreach (var prophecy in _config.Prophecies.Where(prophecy => prophecy.Epoch == index))
                _pending.Enqueue(prophecy);
        }

        private void OnAbandoned(Godland.Core.World.Grid.GridCoord cell) => _context.Record.Count("abandoned");

        private void OnHeroTriumphed(Hero hero)
        {
            if (hero.Quest == QuestKind.God) _context.Record.CountGodSlain();
        }
    }
}
