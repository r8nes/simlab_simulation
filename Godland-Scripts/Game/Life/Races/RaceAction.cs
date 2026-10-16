using System;
using System.Collections.Generic;
using Godland.Game.Fate.Emergence;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Life.Races
{
    [Serializable]
    public sealed class RaceAction
    {
        [field: SerializeField, Required] public string Name { get; private set; }
        [field: SerializeField, Range(1, 100), InfoBox("Насколько это действие в нраве народа. Дальше разум учится сам.")]
        public int Nature { get; private set; } = 50;
        [field: SerializeField, InfoBox("Правило мира: где действие вообще возможно.")]
        public RaceActionGate Gate { get; private set; }
        [SerializeReference] private List<IEmergenceOutcome> _outcomes = new();

        public IReadOnlyList<IEmergenceOutcome> Outcomes => _outcomes;
    }
}
