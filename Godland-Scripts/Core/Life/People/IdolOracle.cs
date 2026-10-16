using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Grid;
using Godland.Core.Randomness;
using Godland.Core.World.Tiles;

namespace Godland.Core.Life.People
{
    public sealed class IdolOracle
    {
        private const int BiomeRadius = 2;

        private readonly WorldTiles _tiles;
        private readonly Settlements _settlements;
        private readonly IShrineRules _rules;
        private readonly SeededRandom _random;

        public IdolOracle(WorldTiles tiles, Settlements settlements, IShrineRules rules, SeededRandom random)
        {
            _tiles = tiles;
            _settlements = settlements;
            _rules = rules;
            _random = random;
        }

        public string Choose(Religion religion, IReadOnlyCollection<GridCoord> followers)
        {
            if (religion.Shrine is not { } shrine || !_tiles.TryGet(shrine, out var godTileId)) return null;

            var cultures = followers.Select(_settlements.CultureOf).Where(culture => culture != null).Select(culture => culture.Id).ToHashSet();
            var fitting = _rules.Idols.Where(idol => Fits(idol, religion, shrine, godTileId, cultures)).ToList();
            var candidates = fitting.Any(idol => !idol.IsFallback) ? fitting.Where(idol => !idol.IsFallback).ToList() : fitting;
            var total = candidates.Sum(idol => idol.Weight);
            if (total <= 0) return null;

            var roll = _random.Next(total);
            foreach (var idol in candidates)
            {
                if (roll < idol.Weight) return idol.Id;
                roll -= idol.Weight;
            }

            return null;
        }

        private bool Fits(IdolRule idol, Religion religion, GridCoord shrine, string godTileId, HashSet<string> cultures) =>
            idol.Weight > 0
            && (idol.GodTileIds.Count == 0 || idol.GodTileIds.Contains(godTileId))
            && (idol.CultureIds.Count == 0 || idol.CultureIds.Any(cultures.Contains))
            && (idol.DogmaIds.Count == 0 || idol.DogmaIds.Contains(religion.Dogma.Id))
            && (idol.BiomeTileId == null || CountAround(shrine, idol.BiomeTileId) >= idol.BiomeMinCount)
            && religion.Hunts >= idol.MinHunts
            && (idol.MaxHunts < 0 || religion.Hunts <= idol.MaxHunts)
            && religion.Losses >= idol.MinLosses;

        private int CountAround(GridCoord center, string tileId)
        {
            var count = 0;
            for (var dy = -BiomeRadius; dy <= BiomeRadius; dy++)
            for (var dx = -BiomeRadius; dx <= BiomeRadius; dx++)
                if (_tiles.TryGet(new GridCoord(center.X + dx, center.Y + dy), out var id) && id == tileId)
                    count++;
            return count;
        }
    }
}
