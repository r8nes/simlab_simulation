using System.Collections.Generic;

namespace Godland.Core.Life.People
{
    public sealed class IdolRule
    {
        public IdolRule(
            string id, int weight, IReadOnlyCollection<string> godTileIds, IReadOnlyCollection<string> cultureIds,
            IReadOnlyCollection<string> dogmaIds, string biomeTileId, int biomeMinCount, int minHunts, int maxHunts, int minLosses,
            bool isFallback)
        {
            IsFallback = isFallback;
            Id = id;
            Weight = weight;
            GodTileIds = godTileIds;
            CultureIds = cultureIds;
            DogmaIds = dogmaIds;
            BiomeTileId = biomeTileId;
            BiomeMinCount = biomeMinCount;
            MinHunts = minHunts;
            MaxHunts = maxHunts;
            MinLosses = minLosses;
        }

        public string Id { get; }
        public int Weight { get; }
        public IReadOnlyCollection<string> GodTileIds { get; }
        public IReadOnlyCollection<string> CultureIds { get; }
        public IReadOnlyCollection<string> DogmaIds { get; }
        public string BiomeTileId { get; }
        public int BiomeMinCount { get; }
        public int MinHunts { get; }
        public int MaxHunts { get; }
        public int MinLosses { get; }
        public bool IsFallback { get; }
    }
}
