using System.Collections.Generic;
using System.Linq;
using Godland.Core.Powers.Titans;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Powers.Titans
{
    [CreateAssetMenu(menuName = "Godland/Titan", fileName = "Titan")]
    public sealed class TitanDefinition : ScriptableObject
    {
        [field: SerializeField, Required]
        public string Id { get; private set; }

        [field: SerializeField, Required]
        public string DisplayName { get; private set; }

        [field: SerializeField, TextArea]
        public string Description { get; private set; }

        [field: SerializeField, BoxGroup("View")]
        public Color Color { get; private set; } = Color.white;

        [field: SerializeField, BoxGroup("View"), MinValue(0.1f)]
        public float Scale { get; private set; } = 1.4f;

        [SerializeField, BoxGroup("Behaviour"), MinValue(1), SuffixLabel("years")] private int _stepPeriod = 15;
        [SerializeField, BoxGroup("Behaviour"), MinValue(1), SuffixLabel("years")] private int _lifespan = 400;
        [SerializeField, BoxGroup("Behaviour")] private TitanTarget _target;
        [SerializeField, BoxGroup("Behaviour"), MinValue(0), InfoBox("Сколько жителей теряют поселения на его пути и рядом.")] private int _harm = 1;

        [SerializeField, BoxGroup("Trail"), AssetsOnly, InfoBox("Во что превращается земля под ним (пусто — не меняет).")] private TileDefinition _trail;
        [SerializeField, BoxGroup("Trail"), AssetsOnly, InfoBox("Во что превращается поселение под ним (пусто — как остальная земля).")] private TileDefinition _settlementTile;
        [SerializeField, BoxGroup("Trail"), AssetsOnly, InfoBox("Эти тайлы он не трогает.")] private List<TileDefinition> _spare = new();
        [SerializeField, BoxGroup("Trail"), AssetsOnly, InfoBox("Через сколько лет след возрождается во что (пусто — не возрождается).")] private TileDefinition _regrow;
        [SerializeField, BoxGroup("Trail"), MinValue(0), SuffixLabel("years")] private int _regrowYears = 20;

        [SerializeField, BoxGroup("Trail"), AssetsOnly, InfoBox("Облики: если заданы, след меняется по кругу раз в период (Химера).")] private List<TileDefinition> _forms = new();
        [SerializeField, BoxGroup("Trail"), MinValue(1), SuffixLabel("years")] private int _shiftPeriod = 50;
        [SerializeField, BoxGroup("Behaviour"), InfoBox("Несёт мор на своём пути; неуязвимые к мору могут его одолеть.")] private bool _infects;

        [SerializeField, BoxGroup("Death"), MinValue(0), SuffixLabel("years"), InfoBox("Его гибель приносит долгую ночь (0 — нет).")] private int _deathNightYears;

        private TitanRule _rule;

        public TitanRule Rule => _rule ??= new TitanRule(Id, _stepPeriod, _lifespan, _target, _trail != null ? _trail.Id : null,
            _spare.Select(tile => tile.Id).ToList(), _settlementTile != null ? _settlementTile.Id : null, _harm,
            _regrow != null ? _regrow.Id : null, _regrowYears, _deathNightYears, _forms.Select(tile => tile.Id).ToList(), _shiftPeriod, _infects);
    }
}
