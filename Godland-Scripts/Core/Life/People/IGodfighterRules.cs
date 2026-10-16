namespace Godland.Core.Life.People
{
    public interface IGodfighterRules
    {
        int IreThreshold { get; }
        int IreDecayPeriod { get; }
        int RevoltChancePercent { get; }
        int GodfighterPeriod { get; }
        int GodfighterSpreadChancePercent { get; }
    }
}
