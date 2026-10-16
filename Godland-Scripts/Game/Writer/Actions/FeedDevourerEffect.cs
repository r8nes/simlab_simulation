using System;
using Godland.Core.World.Grid;

namespace Godland.Game.Writer.Actions
{
    [Serializable]
    public sealed class FeedDevourerEffect : ITileEffect
    {
        public string Describe() => "Пожиратель принимает веру вместо земли; накорми трижды — и бездна закроется";

        public bool CanAffect(GridCoord target, TileEffectContext context) => context.Devourer.IsActive && context.Devourer.Maw == target;

        public void Apply(GridCoord target, TileEffectContext context) => context.Devourer.Feed();
    }
}
