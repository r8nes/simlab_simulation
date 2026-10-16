namespace Godland.Core.World.Cells
{
    public interface IOverflowRules
    {
        int OverflowThreshold { get; }
        string OverflowResult(string tileId, int epoch);
        int ThresholdFor(string tileId, int epoch);
    }
}
