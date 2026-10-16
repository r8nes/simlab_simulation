using System.Collections.Generic;

namespace Godland.Core.World.Climate
{
    public interface IClimateRules
    {
        bool CanDryOut(string tileId);
        bool IsWater(string tileId);
        string DesertTileId { get; }
        int DesertYears { get; }
        int DesertFromEpoch { get; }
        bool IsPeak(string tileId);
        int GlacierMinPeaks { get; }
        bool CanFreeze(string tileId);
        string SnowTileId { get; }
        int GlacierPeriod { get; }
        bool IsSpreadingForest(string tileId);
        bool CanBecomeForest(string tileId);
        string ForestTileId { get; }
        int ForestSpreadPeriod { get; }
        int ForestSpreadChancePercent { get; }
        string SeaTileId { get; }
        bool IsAbsorbedBySea(string tileId);
        int SeaSpreadPeriod { get; }
        int SeaSpreadChancePercent { get; }
        bool BreedsWildlife(string tileId);
        int WildlifePeriod { get; }
        IReadOnlyList<string> WildlifeIds { get; }
    }
}
