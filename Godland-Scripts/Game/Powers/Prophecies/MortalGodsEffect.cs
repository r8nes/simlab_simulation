using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Powers.Prophecies
{
    [Serializable]
    public sealed class MortalGodsEffect : IMilestoneEffect
    {
        [SerializeField, MinValue(1), SuffixLabel("years")] private int _lifespan = 200;

        public void Apply(ProphecyContext context) => context.Pantheon.MakeMortal(_lifespan);
    }
}
