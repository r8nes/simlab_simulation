using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Anomalies
{
    [Serializable]
    public sealed class DriftAnomaly : IAnomalyEffect
    {
        [SerializeField, MinValue(1), SuffixLabel("years")] private int _period = 20;
        [SerializeField, MinValue(1), SuffixLabel("years")] private int _duration = 120;

        public bool CanBegin(AnomalyContext context) => !context.Drift.IsActive && context.Tiles.CountedCount >= 12;

        public void Begin(AnomalyContext context, int year) => context.Drift.Begin(_period, year + _duration);
    }
}
