namespace Godland.Core.World.Tiles
{
    public interface ITileRules
    {
        bool HasFertility(string tileId);
        int GetNeighborFertility(string tileId);
        bool TryGetCreationYield(string tileId, out int amount, out int periodYears);
    }
}
