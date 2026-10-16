using System.Collections.Generic;

namespace Godland.Core.World.Biomes
{
    public interface ISuperBiomeRules
    {
        int DominancePercent { get; }
        int MinTiles { get; }
        IReadOnlyList<SuperBiomeRule> Biomes { get; }
    }
}
