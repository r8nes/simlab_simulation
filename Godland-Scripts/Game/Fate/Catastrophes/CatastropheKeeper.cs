using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core;

namespace Godland.Game.Fate.Catastrophes
{
    public sealed class CatastropheKeeper : IWorldStep
    {
        private readonly Godland.Core.Flow.FlowBias _flow;
        private readonly CatastropheConfig _config;
        private readonly CatastropheContext _context;
        private readonly Dictionary<CatastropheDefinition, int> _counts = new();
        private int _ageBegan;
        private int _calmUntil;

        public CatastropheKeeper(CatastropheConfig config, CatastropheContext context, Godland.Core.Flow.FlowBias flow)
        {
            _flow = flow;
            _config = config;
            _context = context;
        }

        public event Action<CatastropheDefinition, int> Foretold;
        public event Action<CatastropheDefinition> Averted;
        public event Action<CatastropheDefinition> Struck;
        public event Action<AgeDefinition> AgeBegan;
        public event Action<AgeDefinition> AgeEnded;

        public CatastropheDefinition Coming { get; private set; }
        public int StrikeYear { get; private set; }
        public AgeDefinition Age { get; private set; }

        public bool WasDelayed { get; private set; }

        public bool Delay(int years)
        {
            if (Coming == null || WasDelayed) return false;

            WasDelayed = true;
            StrikeYear += years;
            return true;
        }

        public int AgeYearsLeft(int year) => Age == null ? 0 : _ageBegan + Age.Years - year;

        public void Advance(int year)
        {
            if (Age == null) _context.TrackPeak();
            if (Age != null) LiveAge(year);
            if (Coming != null && year >= StrikeYear) Resolve();
            if (Coming == null && year % _config.CheckPeriod == 0) Watch(year, Age != null || year < _calmUntil);
        }

        private void Watch(int year, bool calm)
        {
            foreach (var catastrophe in _config.Catastrophes)
            {
                if (calm && !catastrophe.IsUrgent) continue;
                if (_counts.GetValueOrDefault(catastrophe) >= catastrophe.MaxPerRun) continue;
                if (_context.World.Epoch.Index < catastrophe.MinEpoch || !catastrophe.Trigger.IsReady(_context)) continue;
                if (_context.World.Random.Next(100) >= _flow.Chance($"catastrophe:{catastrophe.name}", catastrophe.ChancePercent)) continue;

                Coming = catastrophe;
                StrikeYear = year + catastrophe.WarningYears;
                _counts[catastrophe] = _counts.GetValueOrDefault(catastrophe) + 1;
                _flow.Note($"catastrophe:{catastrophe.name}");
                Foretold?.Invoke(catastrophe, StrikeYear);
                if (catastrophe.WarningYears == 0) Resolve();
                return;
            }
        }

        private void Resolve()
        {
            var catastrophe = Coming;
            Coming = null;
            if (catastrophe.CanBeAverted && !catastrophe.Trigger.IsReady(_context))
            {
                Averted?.Invoke(catastrophe);
                return;
            }

            foreach (var strike in catastrophe.Strikes) strike.Strike(_context, catastrophe.Title.ToLowerInvariant());
            var memory = _context.World.Memory;
            memory.Add(_context.World.Grid.Center, $"catastrophe:{catastrophe.name}");
            memory.Add(_context.World.Grid.Center, "catastrophes");
            Struck?.Invoke(catastrophe);
            BeginAge(catastrophe);
        }

        private void BeginAge(CatastropheDefinition catastrophe)
        {
            if (Age != null)
                foreach (var law in Age.Laws) law.End(_context);
            Age = catastrophe.Aftermath;
            _ageBegan = _context.World.Clock.Year;
            foreach (var law in Age.Laws) law.Begin(_context, Age.DisplayName.ToLowerInvariant());
            AgeBegan?.Invoke(Age);
        }

        private void LiveAge(int year)
        {
            var age = year - _ageBegan;
            if (age >= Age.Years)
            {
                var ended = Age;
                foreach (var law in ended.Laws) law.End(_context);
                Age = null;
                _calmUntil = year + _config.CooldownYears;
                AgeEnded?.Invoke(ended);
                return;
            }

            if (age > 0)
                foreach (var law in Age.Laws) law.Year(_context, age, Age.DisplayName.ToLowerInvariant());
        }
    }
}
