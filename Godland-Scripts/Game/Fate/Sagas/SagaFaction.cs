using System.Collections.Generic;
using Godland.Core.World.Grid;
using Godland.Core.Life.Minds;

namespace Godland.Game.Fate.Sagas
{
    public sealed class SagaFaction
    {
        public SagaFaction(SagaFactionDefinition definition, Mind mind, int side)
        {
            Definition = definition;
            Mind = mind;
            Side = side;
        }

        public SagaFactionDefinition Definition { get; }
        public Mind Mind { get; }
        public int Side { get; }
        public int LeaderIndex { get; set; }
        public int Bonus { get; set; }
        public int Power { get; set; }
        public bool HasArtifact { get; set; }
        public string LastDeed { get; set; }
        public HashSet<GridCoord> Lands { get; } = new();

        public string Leader
        {
            get
            {
                var names = Definition.LeaderNames;
                var name = names.Count == 0 ? Definition.LeaderTitle : names[LeaderIndex % names.Count];
                return string.IsNullOrEmpty(Definition.LeaderTitle) || name == Definition.LeaderTitle ? name : $"{Definition.LeaderTitle} {name}";
            }
        }
    }
}
