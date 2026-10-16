using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.World.Grid
{
    [CreateAssetMenu(menuName = "Godland/Grid Config", fileName = "GridConfig")]
    public sealed class GridConfig : ScriptableObject
    {
        [field: SerializeField, BoxGroup("Size"), MinValue(1)]
        public int Width { get; private set; } = 7;

        [field: SerializeField, BoxGroup("Size"), MinValue(1)]
        public int Height { get; private set; } = 7;

        [field: SerializeField, BoxGroup("Size"), MinValue(0)]
        public int InitialUnlockedSize { get; private set; } = 3;

        [field: SerializeField, MinValue(1), InfoBox("Размер самого мира внутри сетки. Остальное — запас за краем: туда уходит земля, вытесненная хребтами.")]
        public int WorldSize { get; private set; } = 7;

        [field: SerializeField, BoxGroup("Layout"), MinValue(0.1f)]
        public float CellSize { get; private set; } = 2f;

        [field: SerializeField, BoxGroup("Reveal"), Range(0f, 1f)]
        public float IdleVisibility { get; private set; } = 0.15f;

        [field: SerializeField, BoxGroup("Reveal"), MinValue(0.01f), SuffixLabel("1/s")]
        public float RevealSpeed { get; private set; } = 4f;

        [field: SerializeField, BoxGroup("Reveal"), Range(0f, 1f)]
        public float BlockedHighlight { get; private set; } = 0.25f;
    }
}
