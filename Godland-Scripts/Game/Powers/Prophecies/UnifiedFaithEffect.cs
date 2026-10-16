using System;
using System.Linq;

namespace Godland.Game.Powers.Prophecies
{
    [Serializable]
    public sealed class UnifiedFaithEffect : IMilestoneEffect
    {
        public void Apply(ProphecyContext context)
        {
            var shrines = context.Shrines;
            var dominant = shrines.Religions
                .OrderByDescending(religion => shrines.FollowersOf(religion).Count)
                .FirstOrDefault();
            if (dominant != null) shrines.Unify(dominant);
        }
    }
}
