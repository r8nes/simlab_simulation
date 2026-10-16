using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Grid;

namespace Godland.Core.World.Tiles
{
    public sealed class PlayerLocks
    {
        private readonly List<Func<GridCoord, bool>> _locks = new();

        public void Add(Func<GridCoord, bool> locked) => _locks.Add(locked);

        public bool IsLocked(GridCoord cell) => _locks.Any(locked => locked(cell));
    }
}
