using System;
using Godland.Core.Life.People;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Powers.Sovereigns
{
    [Serializable]
    public sealed class PowerFall : ISovereignFall
    {
        [SerializeField, Range(1, 100), SuffixLabel("%"), InfoBox("Свергнут, если его вера держит меньше этой доли всех верующих мира.")]
        private int _minWorldPercent = 15;

        public string Describe() => $"его вера держит меньше {_minWorldPercent}% верующих мира";

        public bool IsOverthrown(SovereignContext context, Religion claimant) =>
            SovereignPower.PercentOfWorld(context, claimant) < _minWorldPercent;
    }
}
