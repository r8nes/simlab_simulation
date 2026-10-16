using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Climate;
using Godland.Game.Life.Features;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.World.Climate
{
    [CreateAssetMenu(menuName = "Godland/Climate Config", fileName = "ClimateConfig")]
    public sealed class ClimateConfig : ScriptableObject, IClimateRules
    {
        [SerializeField, BoxGroup("Desert"), AssetsOnly, InfoBox("Какая суша может пересохнуть: плодородие держится на нуле и рядом нет воды.")]
        private List<TileDefinition> _dryable = new();

        [SerializeField, BoxGroup("Desert"), AssetsOnly]
        private List<TileDefinition> _water = new();

        [SerializeField, BoxGroup("Desert"), Required, AssetsOnly]
        private TileDefinition _desert;

        [field: SerializeField, BoxGroup("Desert"), MinValue(1), SuffixLabel("years")]
        public int DesertYears { get; private set; } = 100;

        [field: SerializeField, BoxGroup("Desert"), MinValue(0), InfoBox("С какой эпохи земля может пересохнуть. Пересыхает только земля, которая уже была плодородной.")]
        public int DesertFromEpoch { get; private set; } = 1;

        [SerializeField, BoxGroup("Glaciers"), AssetsOnly, InfoBox("Что считается вершиной.")]
        private List<TileDefinition> _peaks = new();

        [field: SerializeField, BoxGroup("Glaciers"), MinValue(1), InfoBox("Сколько вершин рядом нужно горе, чтобы стать высокогорьем.")]
        public int GlacierMinPeaks { get; private set; } = 3;

        [SerializeField, BoxGroup("Glaciers"), AssetsOnly, InfoBox("Какие земли у высокогорья покрываются снегом.")]
        private List<TileDefinition> _freezable = new();

        [SerializeField, BoxGroup("Glaciers"), Required, AssetsOnly]
        private TileDefinition _snow;

        [field: SerializeField, BoxGroup("Glaciers"), MinValue(1), SuffixLabel("years")]
        public int GlacierPeriod { get; private set; } = 40;

        [SerializeField, BoxGroup("Forests"), AssetsOnly, InfoBox("Леса, которые на максимуме плодородия сами разрастаются.")]
        private List<TileDefinition> _spreadingForests = new();

        [SerializeField, BoxGroup("Forests"), AssetsOnly, InfoBox("Куда лес может разрастись.")]
        private List<TileDefinition> _clearings = new();

        [SerializeField, BoxGroup("Forests"), Required, AssetsOnly]
        private TileDefinition _forest;

        [field: SerializeField, BoxGroup("Forests"), MinValue(1), SuffixLabel("years")]
        public int ForestSpreadPeriod { get; private set; } = 60;

        [field: SerializeField, BoxGroup("Forests"), Range(0, 100), SuffixLabel("%")]
        public int ForestSpreadChancePercent { get; private set; } = 50;

        [SerializeField, BoxGroup("Seas"), AssetsOnly, InfoBox("Море со временем поглощает соседнюю стоячую воду.")]
        private TileDefinition _sea;

        [SerializeField, BoxGroup("Seas"), AssetsOnly]
        private List<TileDefinition> _stillWater = new();

        [field: SerializeField, BoxGroup("Seas"), MinValue(1), SuffixLabel("years")]
        public int SeaSpreadPeriod { get; private set; } = 40;

        [field: SerializeField, BoxGroup("Seas"), Range(0, 100), SuffixLabel("%")]
        public int SeaSpreadChancePercent { get; private set; } = 50;

        public string SeaTileId => _sea != null ? _sea.Id : null;

        public bool IsAbsorbedBySea(string tileId) => _stillWater.Any(tile => tile.Id == tileId);

        [SerializeField, BoxGroup("Wildlife"), AssetsOnly, InfoBox("Тайлы, которые сами рождают диких зверей (Древняя чаща).")]
        private List<TileDefinition> _wildlifeSources = new();

        [SerializeField, BoxGroup("Wildlife"), AssetsOnly]
        private List<FeatureDefinition> _wildlife = new();

        [field: SerializeField, BoxGroup("Wildlife"), MinValue(1), SuffixLabel("years")]
        public int WildlifePeriod { get; private set; } = 50;

        public string ForestTileId => _forest.Id;

        public IReadOnlyList<string> WildlifeIds => _wildlife.Select(feature => feature.Id).ToList();

        public bool IsSpreadingForest(string tileId) => _spreadingForests.Any(tile => tile.Id == tileId);

        public bool CanBecomeForest(string tileId) => _clearings.Any(tile => tile.Id == tileId);

        public bool BreedsWildlife(string tileId) => _wildlifeSources.Any(tile => tile.Id == tileId);

        public string DesertTileId => _desert.Id;

        public string SnowTileId => _snow.Id;

        public bool CanDryOut(string tileId) => _dryable.Any(tile => tile.Id == tileId);

        public bool IsWater(string tileId) => _water.Any(tile => tile.Id == tileId);

        public bool IsPeak(string tileId) => _peaks.Any(tile => tile.Id == tileId);

        public bool CanFreeze(string tileId) => _freezable.Any(tile => tile.Id == tileId);
    }
}
