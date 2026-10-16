using System;
using Godland.Core.World.Grid;

namespace Godland.Core.Life.People
{
    public sealed class Religion
    {
        public Religion(GridCoord? shrine, Dogma dogma, bool isHeresy, string godId = null)
        {
            GodId = godId;
            Shrine = shrine;
            Dogma = dogma;
            IsHeresy = isHeresy;
        }

        public GridCoord? Shrine { get; }
        public Dogma Dogma { get; }
        public bool IsHeresy { get; }
        public string GodId { get; }
        public bool IsGod => GodId != null;
        public int Fervor { get; private set; }
        public string IdolId { get; private set; }
        public int Hunts { get; private set; }
        public int Losses { get; private set; }

        public void Kindle(int amount) => Fervor += amount;

        public void Cool(int amount) => Fervor = Math.Max(0, Fervor - amount);

        public void Reveal(string idolId) => IdolId = idolId;

        public void RecordHunt() => Hunts++;

        public void RecordLoss() => Losses++;
    }
}
