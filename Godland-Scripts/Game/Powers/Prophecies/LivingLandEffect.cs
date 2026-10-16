using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Powers.Prophecies
{
    [Serializable]
    public sealed class LivingLandEffect : IMilestoneEffect
    {
        [SerializeField, MinValue(1), SuffixLabel("years")] private int _period = 30;
        [SerializeField, MinValue(1), InfoBox("Сколько тайлов за период захватывают более слабого (по плодородию) соседа.")] private int _spreads = 3;

        public void Apply(ProphecyContext context) => context.LivingLand.Awaken(_period, _spreads);
    }
}
