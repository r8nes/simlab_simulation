using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Emergence
{
    [CreateAssetMenu(menuName = "Godland/Emergence", fileName = "Emergence")]
    public sealed class EmergenceDefinition : ScriptableObject
    {
        [field: SerializeField, Required]
        public string Title { get; private set; }

        [field: SerializeField, TextArea]
        public string Text { get; private set; }

        [field: SerializeField, Range(1, 100), SuffixLabel("%"), InfoBox("Шанс за проверку, если условие выполнено где-то в мире.")]
        public int ChancePercent { get; private set; } = 30;

        [field: SerializeField, MinValue(1)]
        public int MaxPerRun { get; private set; } = 1;

        [field: SerializeField, MinValue(0), InfoBox("Не раньше этой эпохи (номер в EpochConfig).")]
        public int MinEpoch { get; private set; }

        [field: SerializeField, InfoBox("Мимолётное: если шанс не выпал, момент упущен — память тратится и в следующий раз нужно копить заново.")]
        public bool IsFleeting { get; private set; }

        [SerializeReference, Required]
        private IEmergenceCondition _condition;

        [SerializeReference]
        private List<IEmergenceOutcome> _outcomes = new();

        public IEmergenceCondition Condition => _condition;

        public IReadOnlyList<IEmergenceOutcome> Outcomes => _outcomes;
    }
}
