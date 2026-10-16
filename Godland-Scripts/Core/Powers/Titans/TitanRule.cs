using System;
using System.Collections.Generic;

namespace Godland.Core.Powers.Titans
{
    public enum TitanTarget
    {
        Wander,
        Settlements,
        GodSeats,
        Edges,
        Sleep
    }

    public sealed class TitanRule
    {
        public TitanRule(
            string id, int stepPeriod, int lifespan, TitanTarget target, string trailTileId, IReadOnlyCollection<string> spareTileIds,
            string settlementTileId, int harm, string regrowTileId, int regrowYears, int deathNightYears,
            IReadOnlyList<string> forms = null, int shiftPeriod = 50, bool infects = false)
        {
            Id = id;
            StepPeriod = stepPeriod;
            Lifespan = lifespan;
            Target = target;
            TrailTileId = trailTileId;
            SpareTileIds = spareTileIds;
            SettlementTileId = settlementTileId;
            Harm = harm;
            RegrowTileId = regrowTileId;
            RegrowYears = regrowYears;
            DeathNightYears = deathNightYears;
            Forms = forms ?? Array.Empty<string>();
            ShiftPeriod = shiftPeriod;
            Infects = infects;
        }

        public string Id { get; }
        public int StepPeriod { get; }
        public int Lifespan { get; }
        public TitanTarget Target { get; }
        public string TrailTileId { get; }
        public IReadOnlyCollection<string> SpareTileIds { get; }
        public string SettlementTileId { get; }
        public int Harm { get; }
        public string RegrowTileId { get; }
        public int RegrowYears { get; }
        public int DeathNightYears { get; }
        public IReadOnlyList<string> Forms { get; }
        public int ShiftPeriod { get; }
        public bool Infects { get; }
    }
}
