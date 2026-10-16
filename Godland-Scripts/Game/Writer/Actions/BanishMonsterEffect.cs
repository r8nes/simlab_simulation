using System;
using Godland.Core.World.Grid;

namespace Godland.Game.Writer.Actions
{
    [Serializable]
    public sealed class BanishMonsterEffect : ITileEffect
    {
        public string Describe() => "изгоняет чудовище с тайла";

        public bool CanAffect(GridCoord target, TileEffectContext context) => context.Monsters.IsMonster(target);

        public void Apply(GridCoord target, TileEffectContext context) => context.Features.Remove(target);
    }
}
