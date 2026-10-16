namespace Godland.Core.Life.Features
{
    public sealed class InteractionModifiers
    {
        public float CounterChanceMultiplier { get; private set; } = 1f;

        public void MultiplyCounterChance(float multiplier) => CounterChanceMultiplier *= multiplier;
    }
}
