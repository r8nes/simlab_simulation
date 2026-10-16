using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.WorldEvents
{
    [Serializable]
    public sealed class GainFaithEffect : IWorldEventEffect
    {
        [SerializeField, MinValue(1)] private int _amount = 10;

        public bool CanApply(WorldEventContext context) => context.Settlements.All.Count > 0;

        public void Apply(WorldEventContext context) => context.Faith.Add(_amount);
    }
}
