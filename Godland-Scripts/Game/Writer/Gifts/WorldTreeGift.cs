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
    public sealed class WorldTreeGift : IGiftEffect
    {
        [SerializeField, Required, AssetsOnly] private TileDefinition _forest;
        [SerializeField, AssetsOnly, InfoBox("Какие земли зарастают.")] private List<TileDefinition> _overgrown = new();
        [SerializeField, MinValue(1)] private int _radius = 2;

        public bool IsRelevant(GiftContext context) => context.Shrines.Religions.Count > 1;

        public string Describe() => "Лес поднимается вокруг центра, а все веры мира сливаются в самую большую";

        public void Apply(GiftContext context)
        {
            var center = context.Grid.Center;
            foreach (var cell in context.TilesOf(_overgrown.Select(tile => tile.Id).ToList()))
                if (Math.Max(Math.Abs(cell.X - center.X), Math.Abs(cell.Y - center.Y)) <= _radius)
                    context.Transform(cell, _forest.Id, 0);

            var religions = context.Shrines.Religions;
            if (religions.Count == 0) return;

            var largest = religions.OrderByDescending(religion => context.Shrines.FollowersOf(religion).Count).First();
            foreach (var religion in religions.Where(religion => religion != largest))
                foreach (var follower in context.Shrines.FollowersOf(religion))
                    context.Shrines.Impose(follower, largest);
        }
    }
}
