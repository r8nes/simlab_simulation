namespace Godland.Core.World.Volcanoes
{
    public interface IVolcanoRules
    {
        bool IsVolcano(string tileId);
        bool BurnsToAsh(string tileId);
        bool Melts(string tileId);
        string AshTileId { get; }
        string MeltedTileId { get; }
        int CheckPeriod { get; }
        int EruptionChancePercent { get; }
        int ChancePerBelieverPercent { get; }
        int WarningYears { get; }
        int PopulationLoss { get; }
        int MeltPeriod { get; }
    }
}
