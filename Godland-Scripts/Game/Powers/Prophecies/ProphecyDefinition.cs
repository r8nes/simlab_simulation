using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Powers.Prophecies
{
    [CreateAssetMenu(menuName = "Godland/Prophecy", fileName = "Prophecy")]
    public sealed class ProphecyDefinition : ScriptableObject
    {
        [field: SerializeField, Required]
        public string Title { get; private set; }

        [field: SerializeField, MinValue(0), InfoBox("Номер эпохи в EpochConfig, с началом которой звучит пророчество.")]
        public int Epoch { get; private set; }

        [field: SerializeField, MinValue(1), SuffixLabel("years"), InfoBox("Если за это время пророчество не сбылось — оно предотвращено.")]
        public int DurationYears { get; private set; } = 200;

        [SerializeReference, Required, InfoBox("Когда пророчество сбывается.")]
        private IProphecyCondition _condition;

        [field: SerializeField, Required, AssetsOnly]
        public MilestoneDefinition Fulfilled { get; private set; }

        [field: SerializeField, Required, AssetsOnly]
        public MilestoneDefinition Prevented { get; private set; }

        public bool IsMet(ProphecyContext context) => _condition != null && _condition.IsMet(context);
    }
}
