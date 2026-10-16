using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Emergence
{
    [Serializable]
    public sealed class SuperBiomeAgeCondition : IEmergenceCondition, IProgressCondition
    {
        [SerializeField, MinValue(1), SuffixLabel("years"), InfoBox("Сколько лет мир должен прожить в супербиоме подряд.")] private int _years = 100;

        public IEnumerable<GridCoord> Sites(EmergenceContext context) =>
            context.Memory.Get(context.Grid.Center, "superbiome-years") < _years
                ? Enumerable.Empty<GridCoord>()
                : context.Features.All.Keys;

        public (int Value, int Threshold) Progress(EmergenceContext context) => (context.Memory.Get(context.Grid.Center, "superbiome-years"), _years);

        public void Consume(EmergenceContext context, GridCoord site) => context.Memory.Forget(context.Grid.Center, "superbiome-years", 0);
    }
}
