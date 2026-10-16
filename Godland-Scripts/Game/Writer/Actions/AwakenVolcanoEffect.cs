using System;
using Godland.Core.World.Grid;

namespace Godland.Game.Writer.Actions
{
    [Serializable]
    public sealed class AwakenVolcanoEffect : ITileEffect
    {
        public string Describe() => "вулкан пробуждается и скоро извергнется";

        public bool CanAffect(GridCoord target, TileEffectContext context) => context.Volcanoes.CanAwaken(target);

        public void Apply(GridCoord target, TileEffectContext context) => context.Volcanoes.Awaken(target);
    }
}
