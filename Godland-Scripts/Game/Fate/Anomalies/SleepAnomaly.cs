using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Anomalies
{
    [Serializable]
    public sealed class SleepAnomaly : IAnomalyEffect
    {
        [SerializeField, MinValue(1), SuffixLabel("years"), InfoBox("Мир спит: ничего не растёт, не рождается и не умирает. Действуешь только ты.")] private int _duration = 50;

        public bool CanBegin(AnomalyContext context) => context.Sleep.Until == 0;

        public void Begin(AnomalyContext context, int year) => context.Sleep.Sleep(year + _duration);
    }
}
