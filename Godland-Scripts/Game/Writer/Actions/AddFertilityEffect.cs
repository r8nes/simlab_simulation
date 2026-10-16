using System;
using Godland.Core.World.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Writer.Actions
{
    [Serializable]
    public sealed class AddFertilityEffect : ITileEffect
    {
        [SerializeField, MinValue(1)] private int _amount = 2;
        [SerializeField] private bool _includeNeighbors = true;

        public string Describe() => _includeNeighbors
            ? $"+{_amount} плодородия тайлу и соседям"
            : $"+{_amount} плодородия тайлу";

        public bool CanAffect(GridCoord target, TileEffectContext context) =>
            context.Fertility.CanHold(target) && context.Fertility.Get(target) < context.Fertility.Max;

        public void Apply(GridCoord target, TileEffectContext context)
        {
            context.Fertility.Add(target, _amount);
            if (!_includeNeighbors) return;

            foreach (var neighbor in context.Grid.Neighbors(target))
                context.Fertility.Add(neighbor, _amount);
        }
    }
}
