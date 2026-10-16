using Godland.Game.Fate.Catastrophes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Sagas
{
    [CreateAssetMenu(menuName = "Godland/Saga", fileName = "Saga")]
    public sealed class SagaDefinition : ScriptableObject
    {
        [field: SerializeField, Required] public string Title { get; private set; }
        [field: SerializeField, TextArea, InfoBox("Супер-переход: что случилось с миром.")] public string Text { get; private set; }
        [field: SerializeField, MinValue(0)] public int MinEpoch { get; private set; } = 3;
        [field: SerializeField, Range(1, 100), SuffixLabel("%")] public int ChancePercent { get; private set; } = 20;
        [field: SerializeField, InfoBox("Сага начинается только как превращение из другой саги.")] public bool OnlyFromSaga { get; private set; }
        [SerializeReference, HideIf(nameof(OnlyFromSaga))] private ICatastropheTrigger _start;
        [field: SerializeField, Required, AssetsOnly] public SagaActDefinition FirstAct { get; private set; }

        public ICatastropheTrigger Start => _start;
    }
}
