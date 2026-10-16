using Godland.Core.World.Grid;

namespace Godland.Game.Writer.Actions
{
    public interface ITileEffect
    {
        string Describe();
        bool CanAffect(GridCoord target, TileEffectContext context);
        void Apply(GridCoord target, TileEffectContext context);
    }

    public interface ITargetedDescription
    {
        string DescribeAt(GridCoord target, TileEffectContext context);
    }
}
