using System;
using Godland.Core.Life.People;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Powers.Sovereigns
{
    [Serializable]
    public sealed class MiracleCostEffect : ISovereignEffect
    {
        [SerializeField, MinValue(1), SuffixLabel("%")] private int _costPercent = 200;

        public void OnRise(SovereignContext context, Religion claimant) => context.Modifiers.SetMiracleCost(_costPercent);

        public void OnReign(SovereignContext context, int year) { }

        public void OnFall(SovereignContext context) => context.Modifiers.SetMiracleCost(100);
    }
}
