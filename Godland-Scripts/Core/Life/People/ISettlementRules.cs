namespace Godland.Core.Life.People
{
    public interface ISettlementRules
    {
        bool CanHost(string tileId);
        SettlementCulture GetCulture(string tileId);
        int SharedFoodValue(string tileId);
        int MinFertility { get; }
        int SpawnPeriod { get; }
        int SpawnFromEpoch { get; }
        int GrowthPeriod { get; }
        int FaithPeriod { get; }
        int VillagePopulation { get; }
        int StartPopulation { get; }
        int BaseCapacity { get; }
        int CapacityPerFood { get; }
        int MigrationMinPopulation { get; }
        int MigrationSize { get; }
        int HuntPeriod { get; }
        int HuntMinPopulation { get; }
        int HuntYears { get; }
        int RescueMinPopulation { get; }
        int RescueChancePercent { get; }
        int ExodusPeriod { get; }
        int EnergyPerCapacity { get; }
        int UpkeepPerPerson { get; }
        int BirthCost { get; }
        int StarveDebt { get; }
        int MaxReserve { get; }
    }
}
