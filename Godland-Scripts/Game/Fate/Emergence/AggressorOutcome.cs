using System;
using System.Linq;
using Godland.Core.World.Grid;

namespace Godland.Game.Fate.Emergence
{
    [Serializable]
    public sealed class AggressorOutcome : IEmergenceOutcome
    {
        public void Apply(EmergenceContext context, GridCoord site, string title)
        {
            var kingdom = context.Realms.All.FirstOrDefault(candidate => candidate.Capital == site || candidate.Members.Contains(site));
            if (kingdom != null) context.Realms.MakeAggressor(kingdom);
        }
    }
}
