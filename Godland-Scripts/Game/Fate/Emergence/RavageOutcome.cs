using System;
using System.Linq;
using Godland.Core.World.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Emergence
{
    [Serializable]
    public sealed class RavageOutcome : IEmergenceOutcome
    {
        [SerializeField, MinValue(1)] private int _radius = 1;
        [SerializeField, MinValue(1)] private int _loss = 2;
        [SerializeField, MinValue(0), InfoBox("Сколько жителей получает поселение-зачинщик.")] private int _gain = 2;

        public void Apply(EmergenceContext context, GridCoord site, string title)
        {
            foreach (var cell in context.Settlements.All.Keys.Where(cell => cell != site &&
                         Math.Max(Math.Abs(cell.X - site.X), Math.Abs(cell.Y - site.Y)) <= _radius).ToList())
                context.Settlements.Weaken(cell, _loss);
            context.Settlements.Bless(site, _gain);
        }
    }
}
