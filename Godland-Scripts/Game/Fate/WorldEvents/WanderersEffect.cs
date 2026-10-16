using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.WorldEvents
{
    [Serializable]
    public sealed class WanderersEffect : IWorldEventEffect
    {
        [SerializeField, MinValue(1)] private int _population = 2;

        public bool CanApply(WorldEventContext context) => context.Settlements.SettleableCells.Count > 0;

        public void Apply(WorldEventContext context) =>
            context.Settlements.TrySettle(context.Pick(context.Settlements.SettleableCells), _population);
    }
}
