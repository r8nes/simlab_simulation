using System.Collections.Generic;
using Godland.Core.World.Grid;

namespace Godland.Game.Fate.Emergence
{
    public interface IEmergenceCondition
    {
        IEnumerable<GridCoord> Sites(EmergenceContext context);
        void Consume(EmergenceContext context, GridCoord site);
    }
}
