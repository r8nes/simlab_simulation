using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Anomalies
{
    [CreateAssetMenu(menuName = "Godland/Anomaly", fileName = "Anomaly")]
    public sealed class AnomalyDefinition : ScriptableObject
    {
        [field: SerializeField, Required]
        public string Title { get; private set; }

        [field: SerializeField, TextArea, InfoBox("Предвестие — за несколько десятилетий до явления.")]
        public string Omen { get; private set; }

        [field: SerializeField, TextArea]
        public string Description { get; private set; }

        [field: SerializeField, MinValue(0)]
        public int Weight { get; private set; } = 1;

        [SerializeReference, Required]
        private IAnomalyEffect _effect;

        public bool CanBegin(AnomalyContext context) => _effect != null && _effect.CanBegin(context);

        public void Begin(AnomalyContext context, int year) => _effect.Begin(context, year);
    }
}
