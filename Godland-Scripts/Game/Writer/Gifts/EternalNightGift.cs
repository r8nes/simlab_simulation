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
    public sealed class EternalNightGift : IGiftEffect
    {
        [SerializeField, MinValue(1), SuffixLabel("years")] private int _years = 100;

        public bool IsRelevant(GiftContext context) => !context.World.IsNight(context.Clock.Year);

        public string Describe() => $"На {_years} лет: плодородие не растёт, чудовища бьют вдвое сильнее, пыл идолов растёт вдвое быстрее";

        public void Apply(GiftContext context) => context.World.BeginNight(context.Clock.Year + _years);
    }
}
