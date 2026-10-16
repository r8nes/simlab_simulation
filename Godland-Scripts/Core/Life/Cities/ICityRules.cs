namespace Godland.Core.Life.Cities
{
    public interface ICityRules
    {
        int CitiesFromEpoch { get; }
        int CityDelayYears { get; }
        int RisePeriod { get; }
        int CityPopulation { get; }
        int CityMinNeighbors { get; }
        int CityMinAge { get; }
        int CityFallPopulation { get; }
        int CityFaithMultiplier { get; }
        int BuildPeriod { get; }
        string BuildResult(string tileId);
    }
}
