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
    public sealed class SkyBridgeGift : IGiftEffect
    {
        [SerializeField, AssetsOnly, InfoBox("Из чего случайно складываются новые земли.")] private List<TileDefinition> _nature = new();

        public bool IsRelevant(GiftContext context) => context.Grid.Coords.Any(context.Grid.IsPurchasable);

        public string Describe() => "Вокруг мира открывается целое кольцо новых земель с дикой природой";

        public void Apply(GiftContext context)
        {
            foreach (var cell in context.Grid.UnlockRing())
                context.Tiles.Place(cell, context.Pick(_nature).Id, "дар");
        }
    }
}
