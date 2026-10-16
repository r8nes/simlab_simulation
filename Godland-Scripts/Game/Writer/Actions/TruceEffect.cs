using System;
using System.Linq;
using Godland.Core.World.Grid;

namespace Godland.Game.Writer.Actions
{
    [Serializable]
    public sealed class TruceEffect : ITileEffect
    {
        public string Describe() => "война окончена";

        public bool CanAffect(GridCoord target, TileEffectContext context) =>
            context.Realms.TryGetAt(target, out var kingdom) && kingdom.Capital == target && context.Realms.IsAtWar(kingdom);

        public void Apply(GridCoord target, TileEffectContext context)
        {
            if (!context.Realms.TryGetAt(target, out var kingdom)) return;

            context.Realms.TryMakePeace(kingdom);
        }
    }
}
