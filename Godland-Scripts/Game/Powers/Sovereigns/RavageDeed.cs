using System;
using System.Linq;
using Godland.Core.Life.People;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Powers.Sovereigns
{
    [Serializable]
    public sealed class RavageDeed : ISovereignDeed
    {
        [SerializeField, MinValue(1)] private int _settlements = 3;
        [SerializeField, MinValue(1)] private int _loss = 2;

        public string Name => "разоряет иноверцев";

        public int Score(SovereignContext context, Religion claimant, Temperament temperament) =>
            Targets(context, claimant).Any() ? temperament.Cruelty + SovereignPower.Rivals(context, claimant) : 0;

        public string Perform(SovereignContext context, Religion claimant)
        {
            var targets = Targets(context, claimant).OrderBy(_ => context.Random.Next(10000)).Take(_settlements).ToList();
            foreach (var settlement in targets) context.Settlements.Weaken(settlement, _loss);
            return $"{Name} ({targets.Count} поселений)";
        }

        private static System.Collections.Generic.IEnumerable<Godland.Core.World.Grid.GridCoord> Targets(SovereignContext context, Religion claimant) =>
            context.Settlements.All.Keys.Where(cell => context.Shrines.TryGet(cell, out var faith) && faith != claimant);
    }
}
