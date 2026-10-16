using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core;

namespace Godland.Game.Fate.Anomalies
{
    public sealed class AnomalyKeeper : IWorldStep
    {
        private readonly AnomalyConfig _config;
        private readonly AnomalyContext _context;
        private readonly HashSet<AnomalyDefinition> _happened = new();
        private int _nextCheck;
        private int _year;

        public AnomalyKeeper(AnomalyConfig config, AnomalyContext context)
        {
            _config = config;
            _context = context;
        }

        public event Action<AnomalyDefinition, int> Foretold;
        public event Action<AnomalyDefinition> Began;

        public AnomalyDefinition Coming { get; private set; }
        public int ComingYear { get; private set; }

        public void Advance(int year)
        {
            _year = year;
            _context.Devourer.Update(year);
            _context.Drift.Update(year);
            _context.Heart.Update(year);
            _context.SkyFolk.Update(year);

            if (Coming != null)
            {
                if (year >= ComingYear) Unleash(year);
                return;
            }

            if (year < Math.Max(_config.StartYear, _nextCheck) || year % _config.CheckPeriod != 0) return;
            if (_happened.Count >= _config.MaxPerRun || _context.Random.Next(100) >= _config.ChancePercent) return;

            var candidates = _config.Anomalies.Where(anomaly => !_happened.Contains(anomaly) && anomaly.CanBegin(_context)).ToList();
            var total = candidates.Sum(anomaly => anomaly.Weight);
            if (total <= 0) return;

            var roll = _context.Random.Next(total);
            Coming = candidates.First(anomaly => (roll -= anomaly.Weight) < 0);
            ComingYear = year + _config.OmenYears;
            _happened.Add(Coming);
            Foretold?.Invoke(Coming, ComingYear);
        }

        public bool CanSummon => _config.Anomalies.Any(anomaly => anomaly.CanBegin(_context));

        public AnomalyDefinition Summon()
        {
            var year = _year;
            var candidates = _config.Anomalies.Where(anomaly => anomaly.CanBegin(_context)).ToList();
            if (candidates.Count == 0) return null;

            var anomaly = candidates[_context.Random.Next(candidates.Count)];
            _happened.Add(anomaly);
            anomaly.Begin(_context, year);
            Began?.Invoke(anomaly);
            return anomaly;
        }

        private void Unleash(int year)
        {
            var anomaly = Coming;
            Coming = null;
            _nextCheck = year + _config.CooldownYears;
            if (!anomaly.CanBegin(_context)) return;

            anomaly.Begin(_context, year);
            Began?.Invoke(anomaly);
        }
    }
}
