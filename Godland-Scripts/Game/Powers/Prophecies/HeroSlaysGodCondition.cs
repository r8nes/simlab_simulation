using System;

namespace Godland.Game.Powers.Prophecies
{
    [Serializable]
    public sealed class HeroSlaysGodCondition : IProphecyCondition
    {
        public bool IsMet(ProphecyContext context) => context.Record.GodsSlainByHeroes > 0;
    }
}
