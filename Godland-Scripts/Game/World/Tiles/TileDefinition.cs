using System.Collections.Generic;
using Godland.Game.World.Evolution;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.World.Tiles
{
    [CreateAssetMenu(menuName = "Godland/Tile Definition", fileName = "Tile")]
    public sealed class TileDefinition : ScriptableObject
    {
        [field: SerializeField, Required]
        public string Id { get; private set; }

        [field: SerializeField]
        public string DisplayName { get; private set; }

        [field: SerializeField, Required, AssetsOnly]
        public TileView Prefab { get; private set; }

        [field: SerializeField, BoxGroup("Cost"), MinValue(0)]
        public int Cost { get; private set; } = 3;

        [field: SerializeField, BoxGroup("Rules")]
        public bool HasFertility { get; private set; } = true;

        [field: SerializeField, BoxGroup("Rules")]
        public int NeighborFertility { get; private set; }

        [field: SerializeField, BoxGroup("Rules"), MinValue(0)]
        public int CreationYield { get; private set; }

        [field: SerializeField, BoxGroup("Rules"), MinValue(1), SuffixLabel("years"), ShowIf(nameof(HasCreationYield))]
        public int CreationYieldPeriod { get; private set; } = 5;

        public bool HasCreationYield => CreationYield > 0;

        [field: SerializeField, BoxGroup("Evolution"), AssetsOnly]
        public List<TileEvolutionDefinition> Evolutions { get; private set; } = new();
    }
}
