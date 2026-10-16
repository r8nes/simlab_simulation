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
    public sealed class InvertWorldGift : IGiftEffect
    {
        [SerializeField, AssetsOnly] private List<TileDefinition> _waters = new();
        [SerializeField, Required, AssetsOnly] private TileDefinition _waterBecomes;
        [SerializeField, AssetsOnly] private List<TileDefinition> _lands = new();
        [SerializeField, Required, AssetsOnly] private TileDefinition _landBecomes;

        public bool IsRelevant(GiftContext context) => context.Any(_waters.Select(tile => tile.Id).ToList()) && context.Any(_lands.Select(tile => tile.Id).ToList());

        public string Describe() => "Вся вода становится сушей, а суша — водой";

        public void Apply(GiftContext context)
        {
            var waters = context.TilesOf(_waters.Select(tile => tile.Id).ToList());
            var lands = context.TilesOf(_lands.Select(tile => tile.Id).ToList());
            foreach (var cell in waters) context.Transform(cell, _waterBecomes.Id, 0);
            foreach (var cell in lands) context.Transform(cell, _landBecomes.Id, 0);
        }
    }
}
