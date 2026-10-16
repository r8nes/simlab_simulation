namespace Godland.Core.Life.Features
{
    public sealed class FeatureEvolutionRule
    {
        public FeatureEvolutionRule(string resultId, string neighborTileId, string neighborFeatureId, float chance)
        {
            ResultId = resultId;
            NeighborTileId = neighborTileId;
            NeighborFeatureId = neighborFeatureId;
            Chance = chance;
        }

        public string ResultId { get; }
        public string NeighborTileId { get; }
        public string NeighborFeatureId { get; }
        public float Chance { get; }
    }
}
