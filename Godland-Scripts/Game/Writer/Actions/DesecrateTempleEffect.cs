using System;
using Godland.Core.World.Grid;

namespace Godland.Game.Writer.Actions
{
    [Serializable]
    public sealed class DesecrateTempleEffect : ITileEffect
    {
        public string Describe() => "храм разрушен: бог слабеет, город теряет жителя";

        public bool CanAffect(GridCoord target, TileEffectContext context) => context.Temples.TryGetAt(target, out _);

        public void Apply(GridCoord target, TileEffectContext context) => context.Temples.Desecrate(target);
    }
}
