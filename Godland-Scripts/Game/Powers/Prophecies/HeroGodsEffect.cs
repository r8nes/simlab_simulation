using System;

namespace Godland.Game.Powers.Prophecies
{
    [Serializable]
    public sealed class HeroGodsEffect : IMilestoneEffect
    {
        public void Apply(ProphecyContext context) => context.Pantheon.OpenToHeroes();
    }
}
