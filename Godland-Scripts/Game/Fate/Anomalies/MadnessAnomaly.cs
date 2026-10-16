using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Anomalies
{
    [Serializable]
    public sealed class MadnessAnomaly : IAnomalyEffect
    {
        [SerializeField, MinValue(1), SuffixLabel("years"), InfoBox("Каждое твоё действие срабатывает на случайный подходящий тайл.")] private int _duration = 50;

        public bool CanBegin(AnomalyContext context) => true;

        public void Begin(AnomalyContext context, int year) => context.Modifiers.Madden(year + _duration);
    }
}
