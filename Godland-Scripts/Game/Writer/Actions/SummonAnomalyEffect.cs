using System;
using Godland.Core.World.Grid;

namespace Godland.Game.Writer.Actions
{
    [Serializable]
    public sealed class SummonAnomalyEffect : ITileEffect
    {
        public string Describe() => "ты сам насылаешь на мир случайное явление — без предвестия";

        public bool CanAffect(GridCoord target, TileEffectContext context) => context.Anomalies.CanSummon;

        public void Apply(GridCoord target, TileEffectContext context) => context.Anomalies.Summon();
    }
}
