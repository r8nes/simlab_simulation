using System;
using System.Linq;
using Godland.Game.World.Evolution;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Writer.Gifts
{
    [Serializable]
    public sealed class UnlockEvolutionGift : IGiftEffect
    {
        [SerializeField, Required, AssetsOnly] private TileDefinition _tile;
        [SerializeField, Required, AssetsOnly] private TileEvolutionDefinition _evolution;

        public bool IsRelevant(GiftContext context) => true;

        public string Describe()
        {
            var condition = _evolution.RequiredNeighbors.Count > 0
                ? ", если рядом " + string.Join(" и ", _evolution.RequiredNeighbors.Select(r => $"«{r.Tile.DisplayName}» ×{r.Count}"))
                : string.Empty;
            return $"новая эволюция: {_tile.DisplayName} → {_evolution.Result.DisplayName}{condition}";
        }

        public void Apply(GiftContext context) => context.Unlocks.UnlockEvolution(_tile, _evolution);
    }
}
