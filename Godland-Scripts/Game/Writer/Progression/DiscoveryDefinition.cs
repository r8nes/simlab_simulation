using Godland.Game.Writer.Commandments;
using Godland.Game.Writer.Run;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Writer.Progression
{
    [CreateAssetMenu(menuName = "Godland/Discovery", fileName = "Discovery")]
    public sealed class DiscoveryDefinition : ScriptableObject
    {
        [field: SerializeField, Required] public string Title { get; private set; }
        [field: SerializeField, TextArea] public string Text { get; private set; }
        [field: SerializeField, InfoBox("Скрытое открытие видно в летописи как «???», пока не случится.")] public bool Hidden { get; private set; }
        [field: SerializeField, ShowIf(nameof(Hidden)), TextArea, InfoBox("Подсказка для скрытого открытия (можно пусто).")] public string Hint { get; private set; }

        [SerializeReference, Required] private IDiscoveryTrigger _trigger;

        [field: SerializeField, BoxGroup("Unlock"), InfoBox("Что открывает первое открытие (необязательно).")] public ProgressionFeature UnlockFeature { get; private set; }
        [field: SerializeField, BoxGroup("Unlock"), AssetsOnly] public CommandmentWord UnlockWord { get; private set; }

        public string Key => _trigger?.Key;
    }
}
