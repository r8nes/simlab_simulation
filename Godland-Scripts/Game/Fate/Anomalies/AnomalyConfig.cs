using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Anomalies
{
    [CreateAssetMenu(menuName = "Godland/Anomaly Config", fileName = "AnomalyConfig")]
    public sealed class AnomalyConfig : ScriptableObject
    {
        [field: SerializeField, MinValue(0), SuffixLabel("год"), InfoBox("Явления из ряда вон: не раньше этого года, не больше MaxPerRun за забег, каждое — один раз.")]
        public int StartYear { get; private set; } = 1300;

        [field: SerializeField, MinValue(1), SuffixLabel("years")]
        public int CheckPeriod { get; private set; } = 50;

        [field: SerializeField, Range(0, 100), SuffixLabel("%")]
        public int ChancePercent { get; private set; } = 40;

        [field: SerializeField, MinValue(1), SuffixLabel("years")]
        public int OmenYears { get; private set; } = 30;

        [field: SerializeField, MinValue(1)]
        public int MaxPerRun { get; private set; } = 2;

        [field: SerializeField, MinValue(0), SuffixLabel("years"), InfoBox("Пауза после явления до следующего предвестия.")]
        public int CooldownYears { get; private set; } = 300;

        [SerializeField, AssetsOnly]
        private List<AnomalyDefinition> _anomalies = new();

        public IReadOnlyList<AnomalyDefinition> Anomalies => _anomalies;
    }
}
