namespace Godland.Game.Fate.WorldEvents
{
    public interface IWorldEventEffect
    {
        bool CanApply(WorldEventContext context);
        void Apply(WorldEventContext context);
    }
}
