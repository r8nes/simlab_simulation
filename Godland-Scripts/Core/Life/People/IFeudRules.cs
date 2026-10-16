namespace Godland.Core.Life.People
{
    public interface IFeudRules
    {
        int FeudPeriod { get; }
        int RaidTension { get; }
        int RaidPopulationLoss { get; }
        int ConquestChancePercent { get; }
    }
}
