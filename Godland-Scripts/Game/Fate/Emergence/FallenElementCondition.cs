using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Emergence
{
    [Serializable]
    public sealed class FallenElementCondition : IEmergenceCondition, IProgressCondition
    {
        [SerializeField, MinValue(1), InfoBox("Сколько раз должен пасть бог одной стихии.")] private int _falls = 3;

        public IEnumerable<GridCoord> Sites(EmergenceContext context)
        {
            if (context.Memory.KeysAt(context.Grid.Center, "god-fell:").Any(mark => mark.Value >= _falls)) yield return context.Grid.Center;
        }

        public (int Value, int Threshold) Progress(EmergenceContext context) =>
            (context.Memory.KeysAt(context.Grid.Center, "god-fell:").Select(mark => mark.Value).DefaultIfEmpty(0).Max(), _falls);

        public void Consume(EmergenceContext context, GridCoord site) { }
    }
}
