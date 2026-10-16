namespace Godland.Game.Powers.Prophecies
{
    public interface IProphecyCondition
    {
        bool IsMet(ProphecyContext context);
    }
}
