using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.WorldEvents
{
    [Serializable]
    public sealed class GainCreationEffect : IWorldEventEffect
    {
        [SerializeField, MinValue(1)] private int _amount = 10;

        public bool CanApply(WorldEventContext context) => true;

        public void Apply(WorldEventContext context) => context.Creation.Add(_amount);
    }
}
