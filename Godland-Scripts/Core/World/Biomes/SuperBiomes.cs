using System;
using System.Linq;
using Godland.Core.World.Cells;
using Godland.Core.Life.Features;
using Godland.Core.World.Grid;
using Godland.Core.Life.People;
using Godland.Core.Randomness;
using Godland.Core.World.Tiles;

namespace Godland.Core.World.Biomes
{
    public sealed class SuperBiomes
    {
        private readonly WorldGrid _grid;
        private readonly WorldTiles _tiles;
        private readonly CellFertility _fertility;
        private readonly Settlements _settlements;
        private readonly FaithPoints _faith;
        private readonly ISuperBiomeRules _rules;
        private readonly SeededRandom _random;
        private readonly TileFeatures _features;
        private readonly WorldModifiers _modifiers;
        private readonly TileGuards _guards;
        private int _year;
        private int _calmUntil;

        public SuperBiomes(
            WorldGrid grid, WorldTiles tiles, CellFertility fertility, Settlements settlements, FaithPoints faith,
            ISuperBiomeRules rules, SeededRandom random, TileFeatures features,
            WorldModifiers modifiers, TileGuards guards)
        {
            _grid = grid;
            _tiles = tiles;
            _fertility = fertility;
            _settlements = settlements;
            _faith = faith;
            _rules = rules;
            _random = random;
            _features = features;
            _modifiers = modifiers;
            _guards = guards;
        }

        public event Action<SuperBiomeRule> Entered;
        public event Action<SuperBiomeRule> Left;
        public event Action<SuperBiomeRule, GridCoord> Transformed;

        public SuperBiomeRule Current { get; private set; }

        public int CalmYearsLeft => Math.Max(0, _calmUntil - _year);

        public bool CanCalm => Current != null && CalmYearsLeft == 0;

        public void Calm(int years) => _calmUntil = _year + years;

        public int SharePercent(SuperBiomeRule biome) =>
            _tiles.CountedCount == 0 ? 0 : _tiles.CountedIds.Count(biome.MemberTileIds.Contains) * 100 / _tiles.CountedCount;

        public SuperBiomeRule Leading => _rules.Biomes.OrderByDescending(SharePercent).FirstOrDefault();

        public void Update(int year)
        {
            _year = year;
            Refresh();
            if (CalmYearsLeft > 0) return;
            if (Current == null || Current.Period <= 0) return;

            var period = Math.Max(1, (int)(Current.Period * _modifiers.SuperBiomePeriodMultiplier));
            if (year % period == 0) Apply(Current);
        }

        private void Refresh()
        {
            var dominant = _tiles.CountedCount < _rules.MinTiles
                ? null
                : _rules.Biomes.FirstOrDefault(biome => SharePercent(biome) >= _rules.DominancePercent);
            if (dominant == Current) return;

            if (Current != null) Left?.Invoke(Current);
            Current = dominant;
            if (Current != null) Entered?.Invoke(Current);
        }

        private void Apply(SuperBiomeRule biome)
        {
            switch (biome.Effect)
            {
                case SuperBiomeEffect.TransformEdge:
                    TransformEdge(biome);
                    break;
                case SuperBiomeEffect.FertilityPulse:
                    foreach (var cell in _tiles.All.Keys.ToList())
                        _fertility.Add(cell, biome.Amount);
                    break;
                case SuperBiomeEffect.FaithPerSettlement:
                    _faith.Add(_settlements.All.Count * biome.Amount);
                    break;
            }
        }

        private bool IsAnchored(GridCoord coord) =>
            _guards.IsProtected(coord) || _settlements.TryGet(coord, out _) || (_features.TryGet(coord, out _) && !_features.IsWild(coord));

        private void TransformEdge(SuperBiomeRule biome)
        {
            var targets = _tiles.All
                .Where(pair => biome.FromTileIds.Contains(pair.Value) && !IsAnchored(pair.Key)
                               && _grid.Neighbors(pair.Key).Any(cell => _tiles.TryGet(cell, out var id) && biome.NearTileIds.Contains(id)))
                .Select(pair => pair.Key)
                .ToList();
            if (targets.Count == 0) return;

            var target = targets[_random.Next(targets.Count)];
            _tiles.Place(target, biome.ToTileId, "супербиом");
            Transformed?.Invoke(biome, target);
        }
    }
}
