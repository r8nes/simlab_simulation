using Godland.Core.Life.Features;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Life.Features
{
    [CreateAssetMenu(menuName = "Godland/Feature Interaction", fileName = "Interaction")]
    public sealed class FeatureInteractionDefinition : ScriptableObject
    {
        [field: SerializeField, Required, AssetsOnly]
        public FeatureDefinition Predator { get; private set; }

        [field: SerializeField, Required, AssetsOnly]
        public FeatureDefinition Prey { get; private set; }

        [field: SerializeField, MinValue(1), SuffixLabel("years")]
        public int Years { get; private set; } = 3;

        [field: SerializeField, BoxGroup("Hidden"), Range(0f, 1f)]
        public float CounterChance { get; private set; } = 0.15f;

        [field: SerializeField, BoxGroup("Hidden"), AssetsOnly]
        public FeatureDefinition Mutant { get; private set; }

        [field: SerializeField, BoxGroup("Texts")]
        public string ThreatText { get; private set; } = "охотятся";

        [field: SerializeField, BoxGroup("Texts")]
        public string PreyLostText { get; private set; } = "съедена";

        [field: SerializeField, BoxGroup("Texts")]
        public string CounterText { get; private set; } = "дала отпор!";

        public FeatureInteraction ToCore() =>
            new(Predator.Id, Prey.Id, Years, CounterChance, Mutant != null ? Mutant.Id : null);
    }
}
