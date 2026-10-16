using Godland.Core.Fate.Epochs;
using Godland.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.Life.Features;
using Godland.Core.Randomness;
using Godland.Game.Writer.Run;

namespace Godland.Game.Writer.Gifts
{
    public sealed class GiftOffers
    {
        private readonly Godland.Core.Flow.FlowBias _flow;
        private readonly RunConfig _config;
        private readonly SeededRandom _random;
        private readonly GiftContext _context;
        private readonly ProgressionUnlocks _progression;
        private readonly WorldEpoch _epoch;
        private readonly HashSet<GiftDefinition> _taken = new();
        private readonly List<GiftDefinition> _current = new();

        public GiftOffers(RunConfig config, SeededRandom random, GiftContext context, ProgressionUnlocks progression, WorldEpoch epoch, Godland.Core.Flow.FlowBias flow)
        {
            _flow = flow;
            _epoch = epoch;
            _config = config;
            _random = random;
            _progression = progression;
            _context = context;
        }

        public event Action Offered;
        public event Action<GiftDefinition> Chosen;

        public IReadOnlyList<GiftDefinition> Current => _current;
        public bool IsOpen => _current.Count > 0;

        public void Offer()
        {
            if (IsOpen) return;

            var candidates = _config.GiftPool.Where(IsAvailable).ToList();
            while (_current.Count < _config.GiftsPerOffer && candidates.Count > 0)
            {
                var gift = PickWeighted(candidates);
                _current.Add(gift);
                candidates.Remove(gift);
            }

            if (IsOpen) Offered?.Invoke();
        }

        public void Choose(GiftDefinition gift)
        {
            if (!_current.Contains(gift)) return;

            _current.Clear();
            _taken.Add(gift);
            foreach (var effect in gift.Effects)
                effect.Apply(_context);

            _flow.Note($"gift:{gift.name}");
            Chosen?.Invoke(gift);
        }

        private bool IsAvailable(GiftDefinition gift) =>
            (gift.Repeatable || !_taken.Contains(gift))
            && _progression.IsUnlocked(gift.RequiredFeature)
            && _epoch.Index >= TierEpoch(gift.Tier)
            && gift.IsRelevant(_context);

        private int TierEpoch(GiftTier tier) => tier switch
        {
            GiftTier.Miracle => _config.MiracleGiftsFromEpoch,
            GiftTier.Shake => _config.ShakeGiftsFromEpoch,
            _ => 0
        };

        private GiftDefinition PickWeighted(List<GiftDefinition> candidates)
        {
            var roll = _random.NextFloat() * candidates.Sum(gift => _flow.Weight($"gift:{gift.name}", gift.Weight));
            foreach (var gift in candidates)
            {
                roll -= _flow.Weight($"gift:{gift.name}", gift.Weight);
                if (roll <= 0f) return gift;
            }

            return candidates[^1];
        }
    }
}
