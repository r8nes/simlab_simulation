using System;
using Godland.Core.World.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Writer.Actions
{
    [Serializable]
    public sealed class WrathEffect : ITileEffect
    {
        [SerializeField, MinValue(0)] private int _population = 3;
        [SerializeField, MinValue(0)] private int _fervor = 15;

        public string Describe() => $"−{_population} жителей, пыл их веры −{_fervor}";

        public bool CanAffect(GridCoord target, TileEffectContext context) =>
            context.Shrines.TryGet(target, out _);

        public void Apply(GridCoord target, TileEffectContext context)
        {
            if (context.Shrines.TryGet(target, out var religion)) religion.Cool(_fervor);
            context.Settlements.Weaken(target, _population);
        }
    }
}
