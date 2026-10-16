using System;

namespace Godland.Game.Powers.Prophecies
{
    [Serializable]
    public sealed class DeathlessWorldEffect : IMilestoneEffect
    {
        public void Apply(ProphecyContext context) => context.Settlements.MakeDeathless();
    }
}
