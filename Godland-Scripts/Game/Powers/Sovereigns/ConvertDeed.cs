using System;
using System.Linq;
using Godland.Core.Life.People;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Powers.Sovereigns
{
    [Serializable]
    public sealed class ConvertDeed : ISovereignDeed
    {
        [SerializeField, MinValue(1)] private int _count = 2;

        public string Name => "обращает соседей в свою веру";

        public int Score(SovereignContext context, Religion claimant, Temperament temperament) =>
            Targets(context, claimant).Any() ? temperament.Pride + (100 - SovereignPower.PercentOfWorld(context, claimant)) / 3 : 0;

        public string Perform(SovereignContext context, Religion claimant)
        {
            var targets = Targets(context, claimant).OrderBy(_ => context.Random.Next(10000)).Take(_count).ToList();
            foreach (var settlement in targets) context.Shrines.Impose(settlement, claimant, BeliefChange.Calling);
            return $"{Name} ({targets.Count})";
        }

        private static System.Collections.Generic.IEnumerable<Godland.Core.World.Grid.GridCoord> Targets(SovereignContext context, Religion claimant) =>
            context.Settlements.All.Keys.Where(cell =>
                !(context.Shrines.TryGet(cell, out var faith) && faith == claimant) && SovereignPower.Borders(context, claimant, cell));
    }
}
