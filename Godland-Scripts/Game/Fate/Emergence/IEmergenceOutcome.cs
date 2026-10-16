using Godland.Core.World.Grid;

namespace Godland.Game.Fate.Emergence
{
    public interface IEmergenceOutcome
    {
        void Apply(EmergenceContext context, GridCoord site, string title);
    }
}
