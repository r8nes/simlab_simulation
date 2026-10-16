using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.World.Tiles
{
    [CreateAssetMenu(menuName = "Godland/Tile Animation Config", fileName = "TileAnimationConfig")]
    public sealed class TileAnimationConfig : ScriptableObject
    {
        [field: SerializeField, BoxGroup("Appear"), MinValue(0.01f), SuffixLabel("s")]
        public float AppearDuration { get; private set; } = 0.35f;

        [field: SerializeField, BoxGroup("Appear")]
        public Ease AppearEase { get; private set; } = Ease.OutBack;

        [field: SerializeField, BoxGroup("Disappear"), MinValue(0.01f), SuffixLabel("s")]
        public float DisappearDuration { get; private set; } = 0.2f;

        [field: SerializeField, BoxGroup("Disappear")]
        public Ease DisappearEase { get; private set; } = Ease.InBack;

        [field: SerializeField, BoxGroup("Pulse"), Range(0f, 1f)]
        public float PulseStrength { get; private set; } = 0.15f;

        [field: SerializeField, BoxGroup("Depleted"), Range(0.1f, 1f), InfoBox("Во сколько раз уменьшается истощённый обитатель.")]
        public float DepletedScale { get; private set; } = 0.5f;

        [field: SerializeField, BoxGroup("Depleted"), Range(0.1f, 1f), InfoBox("Размер дикого (пришедшего самого) обитателя относительно подсаженного.")]
        public float WildScale { get; private set; } = 0.7f;

        [field: SerializeField, BoxGroup("Pulse"), MinValue(0.01f), SuffixLabel("s")]
        public float PulseDuration { get; private set; } = 0.3f;
    }
}
