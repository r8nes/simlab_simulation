using System.Collections.Generic;
using System.Linq;
using Godland.Game.Writer.Run;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Writer.Gifts
{
    [CreateAssetMenu(menuName = "Godland/Gift", fileName = "Gift")]
    public sealed class GiftDefinition : ScriptableObject
    {
        [field: SerializeField, Required]
        public string Id { get; private set; }

        [field: SerializeField]
        public string DisplayName { get; private set; }

        [field: SerializeField, MinValue(0.01f)]
        public float Weight { get; private set; } = 1f;

        [field: SerializeField, InfoBox("Можно получить в забеге несколько раз.")]
        public bool Repeatable { get; private set; } = true;

        [field: SerializeField, InfoBox("Ярус: малые с начала, чудеса и потрясения открываются эпохами (RunConfig).")]
        public GiftTier Tier { get; private set; }

        [field: SerializeField, InfoBox("К какой прокачиваемой системе относится дар. Пока система не прокачана — дар не выпадает. None — базовый.")]
        public ProgressionFeature RequiredFeature { get; private set; }

        [SerializeReference, Required]
        private List<IGiftEffect> _effects = new();

        public IReadOnlyList<IGiftEffect> Effects => _effects;

        public bool IsRelevant(GiftContext context) => _effects.All(effect => effect.IsRelevant(context));

        public string Describe() => string.Join(", ", _effects.Select(effect => effect.Describe()));
    }
}
