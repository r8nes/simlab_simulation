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
    public sealed class RiftGift : IGiftEffect
    {
        [SerializeField, Required, AssetsOnly] private TileDefinition _rift;
        [SerializeField, AssetsOnly] private List<TileDefinition> _immune = new();
        [SerializeField, MinValue(0)] private int _populationLoss = 2;

        public bool IsRelevant(GiftContext context) => context.Tiles.CountedCount >= 9;

        public string Describe() => "Мир раскалывается рекой пополам, вера по ту сторону от святынь рвётся";

        public void Apply(GiftContext context)
        {
            var center = context.Grid.Center;
            var vertical = context.Random.Next(2) == 0;
            var line = context.Tiles.All.Keys
                .Where(cell => vertical ? cell.X == center.X + 1 : cell.Y == center.Y + 1)
                .Where(cell => context.Tiles.TryGet(cell, out var id) && _immune.All(tile => tile.Id != id))
                .ToList();
            foreach (var cell in line) context.Transform(cell, _rift.Id, _populationLoss);

            foreach (var religion in context.Shrines.Religions)
            {
                if (religion.Shrine is not { } shrine) continue;

                var shrineSide = Side(shrine, center, vertical);
                foreach (var follower in context.Shrines.FollowersOf(religion))
                    if (Side(follower, center, vertical) != shrineSide)
                        context.Shrines.Renounce(follower);
            }
        }

        private static int Side(GridCoord cell, GridCoord center, bool vertical) =>
            Math.Sign(vertical ? cell.X - (center.X + 1) : cell.Y - (center.Y + 1));
    }
}
