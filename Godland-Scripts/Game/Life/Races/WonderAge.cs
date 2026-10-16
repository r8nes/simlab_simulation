using System;
using System.Linq;
using Godland.Core;
using Godland.Core.World.Grid;
using Godland.Game.Fate.Anomalies;
using Godland.Game.Fate.Emergence;

namespace Godland.Game.Life.Races
{
    public sealed class WonderAge : IWorldStep
    {
        private readonly RacesConfig _config;
        private readonly RaceKeeper _races;
        private readonly EmergenceContext _context;
        private readonly AnomalyKeeper _anomalies;
        private int _beganYear = -1;

        public WonderAge(RacesConfig config, RaceKeeper races, EmergenceContext context, AnomalyKeeper anomalies)
        {
            _config = config;
            _races = races;
            _context = context;
            _anomalies = anomalies;
        }

        public event Action Began;
        public event Action<Race> Ended;
        public event Action<Race, Race, GridCoord> Clashed;
        public event Action<Race, Race, GridCoord> Befriended;
        public event Action<AnomalyDefinition> Miracle;

        public bool IsActive { get; private set; }
        public bool HasHappened => _beganYear >= 0;
        public int YearsLeft(int year) => IsActive ? _beganYear + _config.WonderYears - year : 0;

        public void Advance(int year)
        {
            if (!HasHappened && _races.Living.Count() >= _config.WonderRaces) Begin(year);
            if (!IsActive) return;

            var age = year - _beganYear;
            if (age % _config.RelationPeriod == 0) Relate();
            if (age > 0 && age % _config.MiraclePeriod == 0 && _anomalies.CanSummon)
            {
                var anomaly = _anomalies.Summon();
                if (anomaly != null) Miracle?.Invoke(anomaly);
            }

            var dominant = Dominant();
            if (age >= _config.WonderYears || dominant != null || _races.Living.Count() < 2) End(dominant);
        }

        private void Begin(int year)
        {
            _beganYear = year;
            IsActive = true;
            _context.Modifiers.SetEmergenceChance(_config.EmergenceChancePercent);
            _context.Memory.Add(_context.Grid.Center, "wonder-age");
            Began?.Invoke();
        }

        private void End(Race dominant)
        {
            IsActive = false;
            foreach (var race in _races.Living) race.IsVeteran = true;
            _context.Modifiers.SetEmergenceChance(100);
            Ended?.Invoke(dominant);
        }

        private Race Dominant()
        {
            var total = _context.Settlements.All.Count;
            if (total == 0) return null;
            return _races.Living.FirstOrDefault(race => race.Cells.Count * 100 >= total * _config.DominancePercent && race.Cells.Count >= 3);
        }

        private void Relate()
        {
            var living = _races.Living.ToList();
            foreach (var race in living)
            foreach (var other in living.Where(other => other != race))
            {
                var meeting = Meeting(race, other);
                if (meeting is not { } pair) continue;

                if (race.Definition.Rivals.Contains(other.Definition))
                {
                    var (attacker, victim) = Population(pair.A) >= Population(pair.B) ? (pair.A, pair.B) : (pair.B, pair.A);
                    _context.Settlements.Weaken(victim, 1);
                    var loser = victim == pair.A ? race : other;
                    loser.Mind.Inherit((loser == race ? other : race).Mind);
                    _races.Absorb(loser, loser == race ? other : race);
                    _context.Settlements.Bless(attacker, 1);
                    _context.Memory.Add(victim, "war");
                    _context.Memory.Add(_context.Grid.Center, "race-war");
                    Clashed?.Invoke(race, other, victim);
                }
                else if (race.Definition.Friends.Contains(other.Definition))
                {
                    _context.Settlements.Bless(pair.A, 1);
                    _context.Memory.Add(pair.A, "race-friendship");
                    Befriended?.Invoke(race, other, pair.A);
                }
            }
        }

        private (GridCoord A, GridCoord B)? Meeting(Race race, Race other)
        {
            foreach (var a in race.Cells)
            foreach (var b in other.Cells)
                if (Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y)) <= _config.RelationRadius) return (a, b);
            return null;
        }

        private int Population(GridCoord cell) => _context.Settlements.TryGet(cell, out var population) ? population : 0;
    }
}
