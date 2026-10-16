using System;
using Godland.Core.World.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Writer.Actions
{
    [Serializable]
    public sealed class AcceptOfferingEffect : ITileEffect
    {
        [SerializeField, MinValue(1)] private int _creation = 10;

        public string Describe() => $"+{_creation} созидания";

        public bool CanAffect(GridCoord target, TileEffectContext context) => context.Settlements.TryGet(target, out _);

        public void Apply(GridCoord target, TileEffectContext context) => context.Creation.Add(_creation);
    }
}
