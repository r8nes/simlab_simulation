using System;
using Godland.Game.Fate.Emergence;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Life.Races
{
    [Serializable]
    public sealed class RaceOrigin
    {
        [field: SerializeField, InfoBox("Как это случилось: строка для хроники и баннера.")] public string Text { get; private set; }
        [field: SerializeField, Range(0, 100), SuffixLabel("%")] public int ChancePercent { get; private set; } = 30;
        [field: SerializeField, MinValue(0)] public int MinEpoch { get; private set; } = 1;
        [SerializeReference, Required] private IEmergenceCondition _condition;

        public IEmergenceCondition Condition => _condition;
    }
}
