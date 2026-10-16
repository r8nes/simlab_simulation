using System;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.WorldEvents
{
    [Serializable]
    public sealed class BlessAllSettlementsEffect : IWorldEventEffect
    {
        [SerializeField, MinValue(1)] private int _population = 1;

        public bool CanApply(WorldEventContext context) => context.Settlements.All.Count > 0;

        public void Apply(WorldEventContext context)
        {
            foreach (var settlement in context.Settlements.All.Keys.ToList())
                context.Settlements.Bless(settlement, _population);
        }
    }
}
