using System;
using Godland.Core.World.Grid;

namespace Godland.Game.Presentation.Inspection
{
    public sealed class TileInspection
    {
        public event Action Changed;

        public GridCoord? Target { get; private set; }

        public bool IsActive => Target.HasValue;

        public void Enter(GridCoord coord)
        {
            if (Target == coord) return;

            Target = coord;
            Changed?.Invoke();
        }

        public void Exit()
        {
            if (!IsActive) return;

            Target = null;
            Changed?.Invoke();
        }
    }
}
