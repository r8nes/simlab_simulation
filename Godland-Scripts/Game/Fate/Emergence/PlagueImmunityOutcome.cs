using System;
using Godland.Core.World.Grid;

namespace Godland.Game.Fate.Emergence
{
    [Serializable]
    public sealed class PlagueImmunityOutcome : IEmergenceOutcome
    {
        public void Apply(EmergenceContext context, GridCoord site, string title) => context.Plague.GrantImmunity(site);
    }
}
