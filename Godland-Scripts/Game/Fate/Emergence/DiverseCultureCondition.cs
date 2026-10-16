using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Emergence
{
    [Serializable]
    public sealed class DiverseCultureCondition : IEmergenceCondition, IProgressCondition
    {
        [SerializeField, MinValue(2), InfoBox("Сколько разных культур должно жить в поселении и вокруг него.")] private int _cultures = 3;

        public IEnumerable<GridCoord> Sites(EmergenceContext context) =>
            context.Settlements.All.Keys.Where(cell => !context.Guards.IsProtected(cell) &&
                context.Grid.Neighbors(cell).Append(cell)
                    .Where(neighbor => context.Settlements.TryGet(neighbor, out _))
                    .Select(neighbor => context.Settlements.CultureOf(neighbor)?.Id)
                    .Where(id => id != null)
                    .Distinct()
                    .Count() >= _cultures);

        public (int Value, int Threshold) Progress(EmergenceContext context)
        {
            var best = context.Settlements.All.Keys
                .Select(cell => context.Grid.Neighbors(cell).Append(cell)
                    .Where(neighbor => context.Settlements.TryGet(neighbor, out _))
                    .Select(neighbor => context.Settlements.CultureOf(neighbor)?.Id)
                    .Where(id => id != null)
                    .Distinct()
                    .Count())
                .DefaultIfEmpty(0).Max();
            return (best, _cultures);
        }

        public void Consume(EmergenceContext context, GridCoord site) { }
    }
}
