using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Grid;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Writer.Gifts
{
    [Serializable]
    public sealed class MoonfallGift : IGiftEffect
    {
        [SerializeField, Required, AssetsOnly] private TileDefinition _crater;
        [SerializeField, Required, AssetsOnly] private TileDefinition _source;
        [SerializeField, Required, AssetsOnly] private TileDefinition _ridge;
        [SerializeField, AssetsOnly] private List<TileDefinition> _immune = new();
        [SerializeField, MinValue(0)] private int _creation = 40;
        [SerializeField, MinValue(0)] private int _populationLoss = 3;

        public bool IsRelevant(GiftContext context) => context.Tiles.CountedCount >= 9;

        public string Describe() => $"Кратер-озеро, Источник порчи и кольцо гор; +{_creation} созидания лунным камнем";

        public void Apply(GiftContext context)
        {
            var targets = context.Tiles.All.Where(pair => _immune.All(tile => tile.Id != pair.Value)).Select(pair => pair.Key).ToList();
            if (targets.Count == 0) return;

            var impact = context.Pick(targets);
            context.Transform(impact, _crater.Id, _populationLoss);

            var rim = context.Grid.Neighbors(impact)
                .Where(cell => context.Tiles.TryGet(cell, out var id) && _immune.All(tile => tile.Id != id))
                .ToList();
            for (var i = 0; i < rim.Count; i++)
                context.Transform(rim[i], i == 0 ? _source.Id : _ridge.Id, _populationLoss);

            context.Creation.Add(_creation);
        }
    }
}
