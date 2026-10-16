using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.Powers.Gods;
using Godland.Game.Life.People;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Powers.Gods
{
    [Serializable]
    public sealed class GodDefinition
    {
        [field: SerializeField, Required]
        public string Id { get; private set; }

        [field: SerializeField]
        public string DisplayName { get; private set; }

        [field: SerializeField]
        public string Title { get; private set; }

        [field: SerializeField, InfoBox("Как называется его деяние: «Прилив», «Рост гор».")]
        public string DeedName { get; private set; }

        [field: SerializeField]
        public Color Color { get; private set; } = Color.white;

        [SerializeField, BoxGroup("Birth"), AssetsOnly, InfoBox("Стихия бога: из этих тайлов он рождается, на одном из них его престол.")]
        private List<TileDefinition> _domain = new();

        [SerializeField, BoxGroup("Birth"), MinValue(1)]
        private int _minDomainTiles = 6;

        [SerializeField, BoxGroup("Birth"), InfoBox("Стихийный бог рождается сам из давления мира. Нестихийный — только тело для богов пантеонов.")]
        private bool _elemental = true;

        [SerializeField, BoxGroup("Birth"), Required, AssetsOnly]
        private DogmaDefinition _dogma;

        [SerializeField, BoxGroup("Deed")]
        private GodActionKind _action;

        [SerializeField, BoxGroup("Deed"), MinValue(1), SuffixLabel("years")]
        private int _period = 60;

        [SerializeField, BoxGroup("Deed"), MinValue(1)]
        private int _radius = 2;

        [SerializeField, BoxGroup("Deed"), MinValue(1), ShowIf(nameof(Transforms))]
        private int _count = 2;

        [SerializeField, BoxGroup("Deed"), AssetsOnly, ShowIf(nameof(Transforms))]
        private List<TileDefinition> _from = new();

        [SerializeField, BoxGroup("Deed"), AssetsOnly, ShowIf(nameof(Transforms))]
        private TileDefinition _to;

        private bool Transforms => _action == GodActionKind.TransformAround;

        public string FullName => string.IsNullOrEmpty(Title) ? DisplayName : $"{DisplayName}, {Title}";

        public GodRule ToCore() =>
            new(Id, _domain.Select(tile => tile.Id).ToList(), _minDomainTiles, _dogma.ToCore(), _action, _period, _radius, _count,
                _from.Select(tile => tile.Id).ToList(), _to != null ? _to.Id : null, _elemental);
    }
}
