using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Grid;

namespace Godland.Core.World.Tiles
{
    public sealed class TileGuards
    {
        private readonly List<Func<GridCoord, bool>> _rules = new();

        public void Add(Func<GridCoord, bool> isProtected) => _rules.Add(isProtected);

        public bool IsProtected(GridCoord coord) => _rules.Any(rule => rule(coord));
    }
}
