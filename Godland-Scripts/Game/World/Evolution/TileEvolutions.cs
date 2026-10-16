using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Cells;
using Godland.Core.World.Grid;
using Godland.Core.World.Tiles;
using Godland.Game.Writer.Run;
using Godland.Game.World.Tiles;

namespace Godland.Game.World.Evolution
{
    public sealed class TileEvolutions
    {
        private readonly PlayerLocks _locks;
        private readonly WorldGrid _grid;
        private readonly WorldTiles _tiles;
        private readonly CellFertility _fertility;
        private readonly TileCatalog _catalog;
        private readonly RunUnlocks _unlocks;
        private readonly TileGuards _guards;

        public TileEvolutions(WorldGrid grid, WorldTiles tiles, CellFertility fertility, TileCatalog catalog, RunUnlocks unlocks, TileGuards guards, PlayerLocks locks)
        {
            _locks = locks;
            _grid = grid;
            _tiles = tiles;
            _fertility = fertility;
            _catalog = catalog;
            _unlocks = unlocks;
            _guards = guards;
        }

        public event Action<GridCoord, TileDefinition> Evolved;

        public IReadOnlyList<TileEvolutionDefinition> OptionsFor(GridCoord coord) =>
            !_locks.IsLocked(coord) && _tiles.TryGet(coord, out var tileId) && !_guards.IsProtected(coord)
                ? _unlocks.EvolutionsFor(_catalog.Get(tileId)).ToList()
                : Array.Empty<TileEvolutionDefinition>();

        public bool IsReady(GridCoord coord, TileEvolutionDefinition evolution) => MissingConditions(coord, evolution).Count == 0;

        public bool HasReadyOption(GridCoord coord) => OptionsFor(coord).Any(evolution => IsReady(coord, evolution));

        public List<string> MissingConditions(GridCoord coord, TileEvolutionDefinition evolution)
        {
            var missing = new List<string>();
            if (_fertility.CanHold(coord) && _fertility.Get(coord) < _fertility.Max)
                missing.Add($"плодородие {_fertility.Max}");

            foreach (var requirement in evolution.RequiredNeighbors)
                if (CountNeighbors(coord, requirement.Tile.Id) < requirement.Count)
                    missing.Add($"рядом «{requirement.Tile.DisplayName}» ×{requirement.Count}");

            return missing;
        }

        public bool TryEvolve(GridCoord coord, TileEvolutionDefinition evolution)
        {
            if (!OptionsFor(coord).Contains(evolution) || !IsReady(coord, evolution)) return false;

            _tiles.Place(coord, evolution.Result.Id, "эволюция");
            Evolved?.Invoke(coord, evolution.Result);
            return true;
        }

        private int CountNeighbors(GridCoord coord, string tileId) =>
            _grid.Neighbors(coord).Count(neighbor => _tiles.TryGet(neighbor, out var id) && id == tileId);
    }
}
