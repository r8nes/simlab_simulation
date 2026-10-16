using System;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Powers.Prophecies
{
    [Serializable]
    public sealed class FaithDominanceCondition : IProphecyCondition
    {
        [SerializeField, Range(1, 100), SuffixLabel("%")] private int _sharePercent = 60;
        [SerializeField, MinValue(1)] private int _minBelievers = 6;

        public bool IsMet(ProphecyContext context)
        {
            var shrines = context.Shrines;
            if (shrines.BelieverCount < _minBelievers) return false;

            var top = shrines.Religions
                .Select(religion => shrines.FollowersOf(religion).Count)
                .DefaultIfEmpty(0)
                .Max();
            return top * 100 >= shrines.BelieverCount * _sharePercent;
        }
    }
}
