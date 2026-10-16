using System.Collections.Generic;

namespace Godland.Game.Fate.Anomalies
{
    public interface IPastWorld
    {
        IReadOnlyList<string> PastTiles { get; }
        IReadOnlyList<string> PastNames { get; }
    }
}
