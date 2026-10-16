using System;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Powers.Prophecies
{
    [Serializable]
    public sealed class BiomeDominanceCondition : IProphecyCondition
    {
        [SerializeField, Range(1, 100), SuffixLabel("%")] private int _sharePercent = 40;
        [SerializeField, MinValue(1)] private int _minTiles = 15;

        public bool IsMet(ProphecyContext context)
        {
            var tiles = context.Tiles.All;
            if (tiles.Count < _minTiles) return false;

            var top = tiles.Values.GroupBy(id => id).Max(group => group.Count());
            return top * 100 >= tiles.Count * _sharePercent;
        }
    }
}
