using System.Collections.Generic;
using System.Linq;
using Godland.Core.Fate.Cataclysms;
using Godland.Core.Life.People;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Life.People
{
    [CreateAssetMenu(menuName = "Godland/Idol", fileName = "Idol")]
    public sealed class IdolDefinition : ScriptableObject
    {
        [field: SerializeField, Required]
        public string Id { get; private set; }

        [field: SerializeField, Required]
        public string DisplayName { get; private set; }

        [field: SerializeField, InfoBox("Эпитет: «Волк Вечной Зимы».")]
        public string Title { get; private set; }

        [field: SerializeField, TextArea, InfoBox("Почему жители пришли именно к нему — показывается игроку.")]
        public string Origin { get; private set; }

        [field: SerializeField, MinValue(0), InfoBox("Вес при броске среди подходящих идолов.")]
        public int Weight { get; private set; } = 1;

        [field: SerializeField, InfoBox("Запасной: выпадает, только если ни один обычный идол не подошёл.")]
        public bool IsFallback { get; private set; }

        [SerializeField, BoxGroup("Conditions"), AssetsOnly, InfoBox("Каким тайлом-богом должна быть святыня. Пусто — любым.")]
        private List<TileDefinition> _godTiles = new();

        [SerializeField, BoxGroup("Conditions"), AssetsOnly, InfoBox("Хотя бы одно поселение этой веры — такой культуры. Пусто — любой.")]
        private List<CultureDefinition> _cultures = new();

        [SerializeField, BoxGroup("Conditions"), AssetsOnly, InfoBox("Догмат религии. Пусто — любой.")]
        private List<DogmaDefinition> _dogmas = new();

        [SerializeField, BoxGroup("Conditions"), AssetsOnly, InfoBox("Биом вокруг святыни (радиус 2). Пусто — не важно.")]
        private TileDefinition _biomeTile;

        [SerializeField, BoxGroup("Conditions"), MinValue(1), ShowIf(nameof(HasBiome))]
        private int _biomeMinCount = 2;

        [SerializeField, BoxGroup("Conditions"), MinValue(0), InfoBox("Сколько охот должны совершить верующие.")]
        private int _minHunts;

        [SerializeField, BoxGroup("Conditions"), MinValue(-1), InfoBox("Не больше стольких охот (мирные боги). −1 — не важно.")]
        private int _maxHunts = -1;

        [SerializeField, BoxGroup("Conditions"), MinValue(0), InfoBox("Сколько поселений этой веры должно вымереть или уйти.")]
        private int _minLosses;

        [field: SerializeField, BoxGroup("Cataclysm"), InfoBox("Что идол призывает, когда держит большую часть мировой веры. None — мирный бог.")]
        public CataclysmKind CataclysmKind { get; private set; }

        [field: SerializeField, BoxGroup("Cataclysm"), HideIf(nameof(IsPeaceful))]
        public string CataclysmName { get; private set; }

        [SerializeField, BoxGroup("Cataclysm"), AssetsOnly, ShowIf(nameof(TransformsTiles))]
        private TileDefinition _cataclysmResult;

        [SerializeField, BoxGroup("Cataclysm"), MinValue(1), ShowIf(nameof(CataclysmKind), CataclysmKind.TransformAroundShrine)]
        private int _cataclysmRadius = 2;

        [SerializeField, BoxGroup("Cataclysm"), AssetsOnly, ShowIf(nameof(TransformsTiles)), InfoBox("Какие тайлы превращаются. Пусто — все, кроме неуязвимых.")]
        private List<TileDefinition> _cataclysmAffected = new();

        private bool IsPeaceful => CataclysmKind == CataclysmKind.None;

        private bool TransformsTiles => CataclysmKind is CataclysmKind.TransformWorld or CataclysmKind.TransformAroundShrine;

        public Cataclysm ToCataclysm() =>
            new(CataclysmKind, _cataclysmResult != null ? _cataclysmResult.Id : null, _cataclysmRadius,
                _cataclysmAffected.Select(tile => tile.Id).ToList());

        private bool HasBiome => _biomeTile != null;

        public string FullName => string.IsNullOrEmpty(Title) ? DisplayName : $"{DisplayName}, {Title}";

        public IdolRule ToCore() =>
            new(Id, Weight,
                _godTiles.Select(tile => tile.Id).ToList(),
                _cultures.Select(culture => culture.Id).ToList(),
                _dogmas.Select(dogma => dogma.Id).ToList(),
                _biomeTile != null ? _biomeTile.Id : null, _biomeMinCount, _minHunts, _maxHunts, _minLosses, IsFallback);
    }
}
