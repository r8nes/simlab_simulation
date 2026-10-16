using System.Collections.Generic;

namespace Godland.Core.Life.People
{
    public sealed class SettlementCulture
    {
        public SettlementCulture(
            string id, int capacityBonus, int faithMultiplier, int neighborFertility,
            IReadOnlyCollection<string> foodTileIds, IReadOnlyList<CultureTransform> transforms,
            IReadOnlyCollection<string> sacredTileIds)
        {
            Id = id;
            CapacityBonus = capacityBonus;
            FaithMultiplier = faithMultiplier;
            NeighborFertility = neighborFertility;
            FoodTileIds = foodTileIds;
            Transforms = transforms;
            SacredTileIds = sacredTileIds;
        }

        public string Id { get; }
        public int CapacityBonus { get; }
        public int FaithMultiplier { get; }
        public int NeighborFertility { get; }
        public IReadOnlyCollection<string> FoodTileIds { get; }
        public IReadOnlyList<CultureTransform> Transforms { get; }
        public IReadOnlyCollection<string> SacredTileIds { get; }
    }
}
