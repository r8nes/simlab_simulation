using System.Collections.Generic;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.World.Evolution
{
    [CreateAssetMenu(menuName = "Godland/Tile Evolution", fileName = "Evolution")]
    public sealed class TileEvolutionDefinition : ScriptableObject
    {
        [field: SerializeField, Required, AssetsOnly]
        public TileDefinition Result { get; private set; }

        [field: SerializeField, BoxGroup("Conditions"), InfoBox("Все условия должны выполняться одновременно. Пусто — без условий по соседям.")]
        public List<NeighborRequirement> RequiredNeighbors { get; private set; } = new();
    }
}
