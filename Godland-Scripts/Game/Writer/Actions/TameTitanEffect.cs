using System;
using Godland.Core.World.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Writer.Actions
{
    [Serializable]
    public sealed class TameTitanEffect : ITileEffect
    {
        [SerializeField, MinValue(0)] private int _guardRadius = 2;

        public string Describe() => $"титан засыпает стражем: земли в радиусе {_guardRadius} мир больше не тронет";

        public bool CanAffect(GridCoord target, TileEffectContext context) =>
            context.Titans.TryGetAt(target, out var titan) && !titan.IsTamed && titan.IsAwake;

        public void Apply(GridCoord target, TileEffectContext context)
        {
            if (context.Titans.TryGetAt(target, out var titan)) context.Titans.Tame(titan, _guardRadius);
        }
    }
}
