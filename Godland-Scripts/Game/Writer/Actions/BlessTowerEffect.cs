using System;
using Godland.Core.World.Grid;

namespace Godland.Game.Writer.Actions
{
    [Serializable]
    public sealed class BlessTowerEffect : ITileEffect
    {
        public string Describe() => "этаж башни сразу; достроенная с благословением — весь мир примет веру строителей";

        public bool CanAffect(GridCoord target, TileEffectContext context) => context.Babel.IsRising && context.Babel.Site == target;

        public void Apply(GridCoord target, TileEffectContext context) => context.Babel.Bless();
    }
}
