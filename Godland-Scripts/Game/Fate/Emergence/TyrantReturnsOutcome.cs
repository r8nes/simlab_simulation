using System;
using Godland.Core.World.Grid;

namespace Godland.Game.Fate.Emergence
{
    [Serializable]
    public sealed class TyrantReturnsOutcome : IEmergenceOutcome
    {
        public void Apply(EmergenceContext context, GridCoord site, string title)
        {
            if (context.Sovereigns.TryReturn(context.Clock.Year)) context.Memory.Forget(context.Grid.Center, "tyrant-fell", 0);
        }
    }
}
