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
    public sealed class DeathOfGodsGift : IGiftEffect
    {
        public bool IsRelevant(GiftContext context) => context.Shrines.Religions.Count > 0;

        public string Describe() => "Все идолы гибнут, их верующие остаются без бога и ищут новую веру";

        public void Apply(GiftContext context)
        {
            foreach (var religion in context.Shrines.Religions)
                foreach (var follower in context.Shrines.FollowersOf(religion))
                    context.Shrines.Renounce(follower);
        }
    }
}
