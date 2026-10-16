using System;
using System.Collections.Generic;
using Godland.Core.World.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Emergence
{
    [Serializable]
    public sealed class GlobalMemoryCondition : IEmergenceCondition, IProgressCondition
    {
        [SerializeField, Required, InfoBox("Память всего мира (хранится в центре): miracles-total, quiet, plague, devoured-land, tyrant-age, emerged:<имя>.")]
        private string _key;
        [SerializeField, MinValue(1)] private int _threshold = 10;
        [SerializeField, InfoBox("Сбросить память после возникновения.")] private bool _consume = true;

        public IEnumerable<GridCoord> Sites(EmergenceContext context)
        {
            if (context.Memory.Get(context.Grid.Center, _key) >= _threshold) yield return context.Grid.Center;
        }

        public (int Value, int Threshold) Progress(EmergenceContext context) => (context.Memory.Get(context.Grid.Center, _key), _threshold);

        public void Consume(EmergenceContext context, GridCoord site)
        {
            if (_consume) context.Memory.Forget(context.Grid.Center, _key, 0);
        }
    }
}
