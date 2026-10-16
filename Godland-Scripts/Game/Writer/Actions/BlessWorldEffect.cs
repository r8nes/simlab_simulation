using System;
using System.Linq;
using Godland.Core.World.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Writer.Actions
{
    [Serializable]
    public sealed class BlessWorldEffect : ITileEffect
    {
        [SerializeField, MinValue(0)] private int _fertility = 2;
        [SerializeField, MinValue(0)] private int _population = 1;

        public string Describe() => $"все земли +{_fertility} плодородия, все поселения +{_population} житель";

        public bool CanAffect(GridCoord target, TileEffectContext context) => context.Settlements.All.Count > 0;

        public void Apply(GridCoord target, TileEffectContext context)
        {
            foreach (var cell in context.Tiles.All.Keys.ToList())
                context.Fertility.Add(cell, _fertility);
            foreach (var settlement in context.Settlements.All.Keys.ToList())
                context.Settlements.Bless(settlement, _population);
        }
    }
}
