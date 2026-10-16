using System;
using Godland.Core.World.Grid;

namespace Godland.Game.Writer.Actions
{
    [Serializable]
    public sealed class RevealGodEffect : ITileEffect
    {
        public string Describe() => "жители обретают бога рядом";

        public bool CanAffect(GridCoord target, TileEffectContext context) => context.Shrines.CanDeify(target);

        public void Apply(GridCoord target, TileEffectContext context) => context.Shrines.Deify(target);
    }
}
