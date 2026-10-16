using System;
using System.Linq;
using UnityEngine;

namespace Godland.Game.Fate.WorldEvents
{
    [Serializable]
    public sealed class FertilityWaveEffect : IWorldEventEffect
    {
        [SerializeField, Tooltip("Отрицательное — засуха.")] private int _amount = 3;

        public bool CanApply(WorldEventContext context) =>
            context.Tiles.All.Keys.Any(cell => context.Fertility.CanHold(cell)
                && (_amount > 0 ? context.Fertility.Get(cell) < context.Fertility.Max : context.Fertility.Get(cell) > 0));

        public void Apply(WorldEventContext context)
        {
            foreach (var cell in context.Tiles.All.Keys.ToList())
                context.Fertility.Add(cell, _amount);
        }
    }
}
