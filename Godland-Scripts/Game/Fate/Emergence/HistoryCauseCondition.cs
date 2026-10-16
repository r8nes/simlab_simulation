using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Emergence
{
    [Serializable]
    public sealed class HistoryCauseCondition : IEmergenceCondition, IProgressCondition
    {
        [SerializeField, Required, InfoBox("Причина в истории тайла, например «великое чудо: материк».")] private string _cause;
        [SerializeField, MinValue(0), SuffixLabel("years")] private int _minAge = 100;

        public IEnumerable<GridCoord> Sites(EmergenceContext context) =>
            context.Tiles.All.Keys.Where(cell => context.History.Of(cell)
                .Any(record => record.Text == _cause && context.Clock.Year - record.Year >= _minAge));

        public (int Value, int Threshold) Progress(EmergenceContext context)
        {
            var best = context.Tiles.All.Keys
                .SelectMany(cell => context.History.Of(cell).Where(record => record.Text == _cause).Select(record => context.Clock.Year - record.Year))
                .DefaultIfEmpty(-1).Max();
            return (Math.Max(0, best), Math.Max(1, _minAge));
        }

        public void Consume(EmergenceContext context, GridCoord site) { }
    }
}
