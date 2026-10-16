using System.Linq;
using System;
using System.Collections.Generic;
using Godland.Core;
using Godland.Core.Life.Minds;
using Godland.Core.Life.People;

namespace Godland.Game.Powers.Sovereigns
{
    public sealed class SovereignKeeper : IWorldStep
    {
        private readonly SovereignConfig _config;
        private readonly SovereignContext _context;
        private readonly IMindRules _mindRules;
        private readonly Dictionary<SovereignDefinition, Mind> _minds = new();
        private Religion _claimant;
        private int _since;

        public SovereignKeeper(SovereignConfig config, SovereignContext context, IMindRules mindRules)
        {
            _mindRules = mindRules;
            _config = config;
            _context = context;
        }

        public event Action<SovereignDefinition> Rose;
        public event Action<SovereignDefinition> Fell;
        public event Action<SovereignDefinition, string> Acted;

        public SovereignDefinition Ruler { get; private set; }

        public Godland.Core.Life.People.Religion Claimant => _claimant;

        public SovereignDefinition LastFallen { get; private set; }

        public Mind MindOf(SovereignDefinition sovereign)
        {
            if (!_minds.TryGetValue(sovereign, out var mind)) _minds[sovereign] = mind = new Mind(_mindRules);
            return mind;
        }

        public bool TryReturn(int year)
        {
            if (Ruler != null || LastFallen == null) return false;

            var claimant = _context.Shrines.Religions.OrderByDescending(religion => _context.Shrines.FollowersOf(religion).Count).FirstOrDefault();
            if (claimant == null) return false;

            Rise(LastFallen, claimant, year);
            return true;
        }

        public void Advance(int year)
        {
            if (Ruler != null)
                foreach (var effect in Ruler.Effects)
                    effect.OnReign(_context, year);

            if (Ruler != null && year % _config.DeedPeriod == 0) Act();
            if (year % _config.CheckPeriod != 0) return;

            if (Ruler == null) TryRise(year);
            else if (year - _since >= _config.MinReignYears && Ruler.IsOverthrown(_context, _claimant)) Fall();
        }

        private void TryRise(int year)
        {
            foreach (var sovereign in _config.Sovereigns)
            {
                if (!sovereign.TryFindClaimant(_context, out var claimant)) continue;

                Rise(sovereign, claimant, year);
                return;
            }
        }

        private void Act()
        {
            var mind = MindOf(Ruler);
            mind.Measure(SovereignPower.PercentOfWorld(_context, _claimant));
            var options = Ruler.Deeds.Select(deed => new MindOption(deed.Name, deed.Score(_context, _claimant, Ruler.Temperament))).ToList();
            if (mind.Decide(options, _minds.Values.ToList(), _context.Random) is not { } decision) return;

            var deed = Ruler.Deeds.First(candidate => candidate.Name == decision.Option.Id);
            Acted?.Invoke(Ruler, $"{deed.Perform(_context, _claimant)} ({decision.Reason})");
        }

        private void Rise(SovereignDefinition sovereign, Religion claimant, int year)
        {
            Ruler = sovereign;
            _claimant = claimant;
            _since = year;
            foreach (var effect in sovereign.Effects) effect.OnRise(_context, claimant);
            Rose?.Invoke(sovereign);
        }

        private void Fall()
        {
            var fallen = Ruler;
            LastFallen = fallen;
            foreach (var effect in fallen.Effects) effect.OnFall(_context);
            Ruler = null;
            _claimant = null;
            Fell?.Invoke(fallen);
        }
    }
}
