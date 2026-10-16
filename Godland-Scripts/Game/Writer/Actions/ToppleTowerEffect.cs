using System;
using Godland.Core.World.Grid;

namespace Godland.Game.Writer.Actions
{
    [Serializable]
    public sealed class ToppleTowerEffect : ITileEffect
    {
        public string Describe() => "башня рушится: город теряет половину жителей, веры по всему миру рвутся";

        public bool CanAffect(GridCoord target, TileEffectContext context) => context.Babel.IsRising && context.Babel.Site == target;

        public void Apply(GridCoord target, TileEffectContext context) => context.Babel.Topple();
    }
}
