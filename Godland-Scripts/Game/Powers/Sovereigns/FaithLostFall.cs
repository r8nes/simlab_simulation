using System;
using Godland.Core.Life.People;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Powers.Sovereigns
{
    [Serializable]
    public sealed class FaithLostFall : ISovereignFall
    {
        [SerializeField, MinValue(0)] private int _maxFollowers;

        public string Describe() => _maxFollowers == 0 ? "в него не верит ни одно поселение" : $"в него верят не больше {_maxFollowers} поселений";

        public bool IsOverthrown(SovereignContext context, Religion claimant) => context.Shrines.FollowersOf(claimant).Count <= _maxFollowers;
    }
}
