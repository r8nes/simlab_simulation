using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.World.Tiles
{
    [CreateAssetMenu(menuName = "Godland/Tile Catalog", fileName = "TileCatalog")]
    public sealed class TileCatalog : ScriptableObject, ITileRules
    {
        [SerializeField, AssetsOnly, ValidateInput(nameof(HasUniqueIds), "Tile ids must be unique")]
        private List<TileDefinition> _tiles = new();

        public IReadOnlyList<TileDefinition> All => _tiles;

        public TileDefinition Get(string id) =>
            _tiles.FirstOrDefault(tile => tile.Id == id)
            ?? throw new KeyNotFoundException($"Tile '{id}' is not in the catalog.");

        public bool HasFertility(string tileId) => Get(tileId).HasFertility;

        public int GetNeighborFertility(string tileId) => Get(tileId).NeighborFertility;

        public bool TryGetCreationYield(string tileId, out int amount, out int periodYears)
        {
            var tile = Get(tileId);
            amount = tile.CreationYield;
            periodYears = tile.CreationYieldPeriod;
            return tile.HasCreationYield;
        }

        private bool HasUniqueIds(List<TileDefinition> tiles) =>
            tiles.Where(tile => tile != null).Select(tile => tile.Id).Distinct().Count() == tiles.Count(tile => tile != null);
    }
}
