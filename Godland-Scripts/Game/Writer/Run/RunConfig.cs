using System.Collections.Generic;
using Godland.Game.Writer.Actions;
using Godland.Game.Life.Features;
using Godland.Game.Writer.Gifts;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Writer.Run
{
    [CreateAssetMenu(menuName = "Godland/Run Config", fileName = "RunConfig")]
    public sealed class RunConfig : ScriptableObject
    {
        [field: SerializeField, BoxGroup("Seed")]
        public bool UseRandomSeed { get; private set; } = true;

        [field: SerializeField, BoxGroup("Seed"), HideIf(nameof(UseRandomSeed))]
        public int Seed { get; private set; }

        [field: SerializeField, BoxGroup("Tiles"), Required, AssetsOnly]
        public TileDefinition StartingTile { get; private set; }

        [field: SerializeField, BoxGroup("Tiles"), Required, AssetsOnly, InfoBox("Временно: тайлы, которые игрок ставит по ЛКМ (переключение Tab).")]
        public List<TileDefinition> PlaceableTiles { get; private set; } = new();

        [field: SerializeField, BoxGroup("Actions"), AssetsOnly, InfoBox("Базовые действия над тайлами — доступны всегда.")]
        public List<TileActionDefinition> BaseActions { get; private set; } = new();

        [field: SerializeField, BoxGroup("Actions"), AssetsOnly, InfoBox("Базовые улучшения (элементы на тайл) — доступны всегда.")]
        public List<FeatureDefinition> BaseUpgrades { get; private set; } = new();

        [field: SerializeField, BoxGroup("Gifts"), AssetsOnly]
        public List<GiftDefinition> GiftPool { get; private set; } = new();

        [field: SerializeField, BoxGroup("Gifts"), MinValue(1)]
        public int GiftsPerOffer { get; private set; } = 3;

        [field: SerializeField, MinValue(0), InfoBox("С какой эпохи (индекс в EpochConfig) выпадают дары-чудеса.")]
        public int MiracleGiftsFromEpoch { get; private set; } = 1;

        [field: SerializeField, MinValue(0), SuffixLabel("%"), InfoBox("Чудеса за веру дорожают на столько процентов с каждой эпохой: поздний мир богаче верой.")]
        public int MiracleCostPerEpochPercent { get; private set; } = 20;

        [field: SerializeField, MinValue(0), InfoBox("С какой эпохи выпадают дары-потрясения.")]
        public int ShakeGiftsFromEpoch { get; private set; } = 2;

        [field: SerializeField, BoxGroup("Gifts"), MinValue(1), SuffixLabel("years")]
        public int GiftIntervalYears { get; private set; } = 150;
    }
}
