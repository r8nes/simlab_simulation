using System;
using System.Linq;
using Godland.Core.World.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Emergence
{
    [Serializable]
    public sealed class GrowOutcome : IEmergenceOutcome
    {
        [SerializeField, MinValue(1)] private int _amount = 2;
        [SerializeField, MinValue(0), InfoBox("Растут и поселения в этом радиусе.")] private int _radius;

        public void Apply(EmergenceContext context, GridCoord site, string title)
        {
            foreach (var cell in context.Settlements.All.Keys.Where(cell =>
                         Math.Max(Math.Abs(cell.X - site.X), Math.Abs(cell.Y - site.Y)) <= _radius).ToList())
            {
                context.Settlements.TryGet(cell, out var population);
                var room = context.Settlements.CapacityOf(cell) - population;
                if (room > 0) context.Settlements.Bless(cell, Math.Min(_amount, room));
            }
        }
    }
}
