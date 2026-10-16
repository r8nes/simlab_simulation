namespace Godland.Core.World.Corruption
{
    public interface ICorruptionRules
    {
        bool IsSource(string tileId);
        bool ResistsCorruption(string tileId);
        string CorruptedTileId { get; }
        string HealedTileId { get; }
        int CorruptionPeriod { get; }
        int CorruptionChancePercent { get; }
        int CorruptionPopulationLoss { get; }
    }
}
