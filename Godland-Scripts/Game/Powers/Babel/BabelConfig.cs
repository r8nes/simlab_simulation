using Godland.Core.Powers.Babel;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Powers.Babel
{
    [CreateAssetMenu(menuName = "Godland/Babel Config", fileName = "BabelConfig")]
    public sealed class BabelConfig : ScriptableObject, IBabelRules
    {
        [field: SerializeField, BoxGroup("Start"), MinValue(0), SuffixLabel("год"), InfoBox("Раз за забег самый большой город может заложить башню до неба, чтобы добраться до богов.")]
        public int StartYear { get; private set; } = 1500;

        [field: SerializeField, BoxGroup("Start"), MinValue(1)]
        public int MinCities { get; private set; } = 3;

        [field: SerializeField, BoxGroup("Start"), MinValue(1)]
        public int MinCityPopulation { get; private set; } = 12;

        [field: SerializeField, BoxGroup("Start"), MinValue(1), SuffixLabel("years")]
        public int CheckPeriod { get; private set; } = 50;

        [field: SerializeField, BoxGroup("Start"), Range(0, 100), SuffixLabel("%")]
        public int ChancePercent { get; private set; } = 30;

        [field: SerializeField, BoxGroup("Build"), MinValue(1), SuffixLabel("years"), InfoBox("Раз в период город или сосед отдаёт жителя — растёт этаж.")]
        public int FloorPeriod { get; private set; } = 20;

        [field: SerializeField, BoxGroup("Build"), MinValue(1)]
        public int Floors { get; private set; } = 10;

        [field: SerializeField, BoxGroup("Outcome"), MinValue(1), SuffixLabel("years"), InfoBox("Достроена без благословения: люди отвергают чудеса: за веру они недоступны столько лет.")]
        public int DefianceYears { get; private set; } = 300;

        [field: SerializeField, BoxGroup("Outcome"), MinValue(1), InfoBox("Достроена с благословением: весь мир принимает веру строителей, города дают во столько раз больше веры.")]
        public int ChildrenFaithMultiplier { get; private set; } = 2;

        [field: SerializeField, BoxGroup("Outcome"), Range(0, 100), SuffixLabel("%"), InfoBox("Башню низвергли: каждое поселение с таким шансом теряет веру.")]
        public int ToppleRenouncePercent { get; private set; } = 50;

        [SerializeField, BoxGroup("Tiles"), Required, AssetsOnly] private TileDefinition _tower;
        [SerializeField, BoxGroup("Tiles"), Required, AssetsOnly] private TileDefinition _ruin;

        public string TowerTileId => _tower.Id;
        public string RuinTileId => _ruin.Id;
    }
}
