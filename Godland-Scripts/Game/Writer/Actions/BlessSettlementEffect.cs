using System;
using Godland.Core.World.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Writer.Actions
{
    [Serializable]
    public sealed class BlessSettlementEffect : ITileEffect
    {
        [SerializeField, MinValue(1)] private int _population = 2;

        public string Describe() => $"+{_population} жителя";

        public bool CanAffect(GridCoord target, TileEffectContext context) => context.Settlements.TryGet(target, out _);

        public void Apply(GridCoord target, TileEffectContext context) => context.Settlements.Bless(target, _population);
    }
}
