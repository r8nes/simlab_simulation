using Godland.Game.Presentation.Settings;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Presentation.Menu
{
    [CreateAssetMenu(menuName = "Godland/Menu Settings", fileName = "MenuSettings")]
    public sealed class MenuSettings : ScriptableObject
    {
        [field: SerializeField, Required, InfoBox("Имя игровой сцены в Build Settings.")]
        public string GameScene { get; private set; } = "Prototype";

        [field: SerializeField, Required, AssetsOnly, InfoBox("Откуда витрина берёт тайлы, обитателей, поселения и титанов.")]
        public GameSettings Game { get; private set; }

        [field: SerializeField, BoxGroup("Showcase"), MinValue(0.5f), SuffixLabel("s")] public float ShowSeconds { get; private set; } = 4f;
        [field: SerializeField, BoxGroup("Showcase"), MinValue(0.1f), SuffixLabel("s")] public float GapSeconds { get; private set; } = 0.6f;
        [field: SerializeField, BoxGroup("Showcase"), MinValue(0.1f)] public float TileSize { get; private set; } = 3f;
        [field: SerializeField, BoxGroup("Showcase"), MinValue(1f), SuffixLabel("s"), InfoBox("Один полный оборот витрины.")]
        public float TurnSeconds { get; private set; } = 20f;
        [field: SerializeField, BoxGroup("Showcase"), Range(0, 100), SuffixLabel("%")] public int FeatureChancePercent { get; private set; } = 70;
        [field: SerializeField, BoxGroup("Showcase"), Range(0, 100), SuffixLabel("%")] public int SettlementChancePercent { get; private set; } = 50;
        [field: SerializeField, BoxGroup("Showcase"), Range(0, 100), SuffixLabel("%")] public int TitanChancePercent { get; private set; } = 15;
        [field: SerializeField, BoxGroup("Showcase"), MinValue(1), InfoBox("С какого числа жителей витрина показывает город.")] public int CityPopulation { get; private set; } = 12;
    }
}
