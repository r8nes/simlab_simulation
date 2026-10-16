namespace Godland.Core.Powers.Babel
{
    public interface IBabelRules
    {
        int StartYear { get; }
        int MinCities { get; }
        int MinCityPopulation { get; }
        int CheckPeriod { get; }
        int ChancePercent { get; }
        int FloorPeriod { get; }
        int Floors { get; }
        int DefianceYears { get; }
        int ChildrenFaithMultiplier { get; }
        int ToppleRenouncePercent { get; }
        string TowerTileId { get; }
        string RuinTileId { get; }
    }
}
