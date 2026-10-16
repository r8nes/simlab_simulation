using System.Collections.Generic;

namespace Godland.Core.World.Biomes
{
    public sealed class SuperBiomeRule
    {
        public SuperBiomeRule(
            string id, IReadOnlyCollection<string> memberTileIds, SuperBiomeEffect effect, int period, int amount,
            IReadOnlyCollection<string> fromTileIds, IReadOnlyCollection<string> nearTileIds, string toTileId)
        {
            Id = id;
            MemberTileIds = memberTileIds;
            Effect = effect;
            Period = period;
            Amount = amount;
            FromTileIds = fromTileIds;
            NearTileIds = nearTileIds;
            ToTileId = toTileId;
        }

        public string Id { get; }
        public IReadOnlyCollection<string> MemberTileIds { get; }
        public SuperBiomeEffect Effect { get; }
        public int Period { get; }
        public int Amount { get; }
        public IReadOnlyCollection<string> FromTileIds { get; }
        public IReadOnlyCollection<string> NearTileIds { get; }
        public string ToTileId { get; }
    }
}
