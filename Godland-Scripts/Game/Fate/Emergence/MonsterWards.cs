using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.Life.Features;
using Godland.Core.World.Grid;

namespace Godland.Game.Fate.Emergence
{
    public sealed class MonsterWards
    {
        private readonly List<Func<GridCoord, bool>> _wards = new();

        public MonsterWards(Monsters monsters)
        {
            monsters.SetWard(cell => _wards.Any(ward => ward(cell)));
        }

        public void Add(Func<GridCoord, bool> ward) => _wards.Add(ward);
    }
}
