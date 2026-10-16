using System.Collections.Generic;
using System.Linq;
using Godland.Game.Presentation.Feedback.Badges;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Presentation.Feedback
{
    [CreateAssetMenu(menuName = "Godland/Feedback Config", fileName = "FeedbackConfig")]
    public sealed class FeedbackConfig : ScriptableObject
    {
        [field: SerializeField, Required, AssetsOnly]
        public FloatingText FloatingTextPrefab { get; private set; }

        [field: SerializeField, MinValue(0.1f), SuffixLabel("s")]
        public float FloatingTextDuration { get; private set; } = 1.4f;

        [field: SerializeField, MinValue(0)]
        public float FloatingTextRise { get; private set; } = 1.5f;

        [field: SerializeField, MinValue(0)]
        public float FloatingTextHeight { get; private set; } = 1f;

        [field: SerializeField, MinValue(0), SuffixLabel("s"), InfoBox("Пауза между всплывашками на одной клетке, чтобы они не наслаивались.")]
        public float PopupInterval { get; private set; } = 0.6f;

        [field: SerializeField, MinValue(1), InfoBox("Сколько всплывашек может ждать в очереди на клетке; лишние угрозы и мелочи отбрасываются.")]
        public int MaxPopupsPerCell { get; private set; } = 3;

        [field: SerializeField, BoxGroup("Badges"), Required, AssetsOnly]
        public TileBadgesView BadgesPrefab { get; private set; }

        [field: SerializeField, BoxGroup("Badges"), MinValue(0), InfoBox("Высота ряда значков над тайлом (в клетках).")]
        public float BadgeHeight { get; private set; } = 0.95f;

        [SerializeField, BoxGroup("Badges"), InfoBox("Круглые значки состояния тайла: символ (или иконка) и цвет.")]
        private List<BadgeDefinition> _badges = new();

        public IReadOnlyList<BadgeDefinition> Badges => _badges;

        public BadgeDefinition BadgeFor(BadgeKind kind) => _badges.First(badge => badge.Kind == kind);

        [field: SerializeField, BoxGroup("Colors")]
        public Color RewardColor { get; private set; } = new(1f, 0.85f, 0.4f);

        [field: SerializeField, BoxGroup("Colors")]
        public Color FertilityColor { get; private set; } = new(0.45f, 1f, 0.5f);

        [field: SerializeField, BoxGroup("Colors")]
        public Color EvolutionColor { get; private set; } = new(0.6f, 0.85f, 1f);

        [field: SerializeField, BoxGroup("Colors")]
        public Color ThreatColor { get; private set; } = new(1f, 0.4f, 0.35f);
    }
}
