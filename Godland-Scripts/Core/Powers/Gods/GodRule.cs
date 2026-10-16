using System.Collections.Generic;
using Godland.Core.Life.People;

namespace Godland.Core.Powers.Gods
{
    public sealed class GodRule
    {
        public GodRule(
            string id, IReadOnlyCollection<string> domainTileIds, int minDomainTiles, Dogma dogma, GodActionKind action,
            int actionPeriod, int radius, int count, IReadOnlyCollection<string> fromTileIds, string toTileId, bool isElemental = true)
        {
            IsElemental = isElemental;
            Id = id;
            DomainTileIds = domainTileIds;
            MinDomainTiles = minDomainTiles;
            Dogma = dogma;
            Action = action;
            ActionPeriod = actionPeriod;
            Radius = radius;
            Count = count;
            FromTileIds = fromTileIds;
            ToTileId = toTileId;
        }

        public string Id { get; }
        public bool IsElemental { get; }
        public IReadOnlyCollection<string> DomainTileIds { get; }
        public int MinDomainTiles { get; }
        public Dogma Dogma { get; }
        public GodActionKind Action { get; }
        public int ActionPeriod { get; }
        public int Radius { get; }
        public int Count { get; }
        public IReadOnlyCollection<string> FromTileIds { get; }
        public string ToTileId { get; }
    }
}
