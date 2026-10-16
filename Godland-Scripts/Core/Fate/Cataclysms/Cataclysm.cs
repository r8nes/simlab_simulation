using System.Collections.Generic;

namespace Godland.Core.Fate.Cataclysms
{
    public sealed class Cataclysm
    {
        public Cataclysm(CataclysmKind kind, string resultTileId, int radius, IReadOnlyCollection<string> affectedTileIds)
        {
            Kind = kind;
            ResultTileId = resultTileId;
            Radius = radius;
            AffectedTileIds = affectedTileIds;
        }

        public CataclysmKind Kind { get; }
        public string ResultTileId { get; }
        public int Radius { get; }
        public IReadOnlyCollection<string> AffectedTileIds { get; }
    }
}
