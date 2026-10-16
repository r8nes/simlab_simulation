using System;
using Godland.Core.World.Grid;

namespace Godland.Game.Writer.Actions
{
    [Serializable]
    public sealed class BlessHeroEffect : ITileEffect
    {
        public string Describe() => "герой удачливее и может бросить вызов чужому богу";

        public bool CanAffect(GridCoord target, TileEffectContext context) =>
            context.Heroes.TryGetAt(target, out var hero) && !hero.Blessed;

        public void Apply(GridCoord target, TileEffectContext context)
        {
            if (context.Heroes.TryGetAt(target, out var hero)) context.Heroes.Bless(hero);
        }
    }
}
