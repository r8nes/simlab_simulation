using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.Randomness;

namespace Godland.Game.Writer.Commandments
{
    public sealed class CommandmentDraft
    {
        private readonly Godland.Core.Flow.FlowBias _flow;
        private readonly CommandmentConfig _config;
        private readonly SeededRandom _random;
        private readonly Run.ProgressionUnlocks _unlocks;

        public CommandmentDraft(CommandmentConfig config, SeededRandom random, Run.ProgressionUnlocks unlocks, Godland.Core.Flow.FlowBias flow)
        {
            _flow = flow;
            _unlocks = unlocks;
            _config = config;
            _random = random;
        }

        public event Action<CommandmentDraft> Offered;

        public IReadOnlyList<CommandmentWord> When { get; private set; } = Array.Empty<CommandmentWord>();
        public IReadOnlyList<CommandmentWord> Then { get; private set; } = Array.Empty<CommandmentWord>();
        public IReadOnlyList<CommandmentWord> Where { get; private set; } = Array.Empty<CommandmentWord>();
        public IReadOnlyList<CommandmentWord> Amplifiers { get; private set; } = Array.Empty<CommandmentWord>();

        public void Offer()
        {
            When = Draw(CommandmentSlot.When, _config.OptionsPerSlot);
            Then = Draw(CommandmentSlot.Then, _config.OptionsPerSlot);
            Where = Draw(CommandmentSlot.Where, _config.OptionsPerSlot);
            Amplifiers = _random.Next(100) < _config.AmplifierChancePercent ? Draw(CommandmentSlot.Amplifier, 1) : Array.Empty<CommandmentWord>();
            Offered?.Invoke(this);
        }

        public Commandment PickRandom() =>
            new(When[_random.Next(When.Count)], Then[_random.Next(Then.Count)], Where[_random.Next(Where.Count)],
                Amplifiers.Count > 0 && _random.Next(2) == 0 ? Amplifiers[0] : null);

        private int WeightOf(CommandmentWord word) => _flow.Weight($"word:{word.name}", _config.WeightOf(word.Rarity));

        private IReadOnlyList<CommandmentWord> Draw(CommandmentSlot slot, int count)
        {
            var pool = _config.Words.Where(word => word.Slot == slot && _unlocks.IsUnlocked(word)).ToList();
            var drawn = new List<CommandmentWord>();
            while (drawn.Count < count && pool.Count > 0)
            {
                var roll = _random.Next(pool.Sum(WeightOf));
                var chosen = pool.First(word => (roll -= WeightOf(word)) < 0);
                _flow.Note($"word:{chosen.name}");
                drawn.Add(chosen);
                pool.Remove(chosen);
            }

            return drawn;
        }
    }
}
