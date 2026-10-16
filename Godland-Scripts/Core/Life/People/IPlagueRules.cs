namespace Godland.Core.Life.People
{
    public interface IPlagueRules
    {
        int PlagueCheckPeriod { get; }
        int PlagueMinRiskFactors { get; }
        int PlagueChancePerFactorPercent { get; }
        int PlagueDuration { get; }
        int PlagueTickPeriod { get; }
        int PlagueSpreadChancePercent { get; }
        int PlagueImmunityYears { get; }
        int RecentRaidYears { get; }
        bool BreedsDisease(string tileId);
    }
}
