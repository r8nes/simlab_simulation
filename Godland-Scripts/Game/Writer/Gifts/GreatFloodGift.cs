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
    public sealed class GreatFloodGift : IGiftEffect
    {
        [SerializeField, AssetsOnly] private List<TileDefinition> _lowlands = new();
        [SerializeField, AssetsOnly] private List<TileDefinition> _waters = new();
        [SerializeField, Required, AssetsOnly] private TileDefinition _flood;
        [SerializeField, MinValue(0)] private int _populationLoss = 3;

        public bool IsRelevant(GiftContext context) => context.Settlements.All.Count > 0 && context.Any(_lowlands.Select(tile => tile.Id).ToList());

        public string Describe() => "Низины у воды уходят под воду; одно поселение спасается в ковчеге";

        public void Apply(GiftContext context)
        {
            var settlements = context.Settlements.All.Keys.ToList();
            GridCoord? ark = settlements.Count > 0 ? context.Pick(settlements) : null;
            var waterIds = _waters.Select(tile => tile.Id).ToList();
            var drowned = context.TilesOf(_lowlands.Select(tile => tile.Id).ToList())
                .Where(cell => cell != ark && context.Grid.Neighbors(cell).Any(n => context.Tiles.TryGet(n, out var id) && waterIds.Contains(id)))
                .ToList();
            foreach (var cell in drowned) context.Transform(cell, _flood.Id, _populationLoss);

        }
    }
}
