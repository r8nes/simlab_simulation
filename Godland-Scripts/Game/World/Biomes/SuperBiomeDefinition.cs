using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Biomes;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.World.Biomes
{
    [Serializable]
    public sealed class SuperBiomeDefinition
    {
        [field: SerializeField, Required]
        public string Id { get; private set; }

        [field: SerializeField]
        public string DisplayName { get; private set; }

        [field: SerializeField, TextArea]
        public string Description { get; private set; }

        [SerializeField, AssetsOnly, InfoBox("Тайлы этого семейства. Их доля в мире решает, наступил ли супербиом.")]
        private List<TileDefinition> _members = new();

        [SerializeField, BoxGroup("Effect")]
        private SuperBiomeEffect _effect;

        [SerializeField, BoxGroup("Effect"), MinValue(1), SuffixLabel("years")]
        private int _period = 30;

        [SerializeField, BoxGroup("Effect"), ShowIf(nameof(UsesAmount))]
        private int _amount = 1;

        [SerializeField, BoxGroup("Effect"), AssetsOnly, ShowIf(nameof(Transforms)), InfoBox("Какие тайлы превращаются…")]
        private List<TileDefinition> _from = new();

        [SerializeField, BoxGroup("Effect"), AssetsOnly, ShowIf(nameof(Transforms)), InfoBox("…если рядом есть такой тайл…")]
        private List<TileDefinition> _near = new();

        [SerializeField, BoxGroup("Effect"), AssetsOnly, ShowIf(nameof(Transforms)), InfoBox("…и во что.")]
        private TileDefinition _to;

        private bool UsesAmount => _effect is SuperBiomeEffect.FertilityPulse or SuperBiomeEffect.FaithPerSettlement;

        private bool Transforms => _effect == SuperBiomeEffect.TransformEdge;

        public SuperBiomeRule ToCore() =>
            new(Id, _members.Select(tile => tile.Id).ToList(), _effect, _period, _amount,
                _from.Select(tile => tile.Id).ToList(), _near.Select(tile => tile.Id).ToList(), _to != null ? _to.Id : null);
    }
}
