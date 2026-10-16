using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Cells;
using Godland.Core.World.Creation;
using Godland.Core.Life.Features;
using Godland.Core.World.Grid;
using Godland.Core.World.Tiles;
using Godland.Game.Writer.Run;

namespace Godland.Game.Life.Features
{
    public sealed class TileUpgrades
    {
        private readonly RunUnlocks _unlocks;
        private readonly WorldTiles _tiles;
        private readonly TileFeatures _features;
        private readonly CreationPoints _creation;
        private readonly CellFertility _fertility;
        private readonly FeatureCatalog _catalog;
        private readonly Godland.Core.World.Peaks.SuperPeaks _peaks;
        private readonly PlayerLocks _locks;

        public TileUpgrades(RunUnlocks unlocks, WorldTiles tiles, TileFeatures features, CreationPoints creation, CellFertility fertility, FeatureCatalog catalog, Godland.Core.World.Peaks.SuperPeaks peaks, PlayerLocks locks)
        {
            _locks = locks;
            _peaks = peaks;
            _unlocks = unlocks;
            _tiles = tiles;
            _features = features;
            _creation = creation;
            _fertility = fertility;
            _catalog = catalog;
        }

        public event Action<GridCoord, FeatureDefinition> Applied;

        public IEnumerable<FeatureDefinition> AvailableFor(GridCoord coord) =>
            _tiles.TryGet(coord, out var tileId) && (!_features.TryGet(coord, out _) || _features.IsWild(coord))
                ? _unlocks.Upgrades.Where(upgrade => upgrade.AppliesTo(tileId) && !IsUseless(upgrade, coord))
                : Enumerable.Empty<FeatureDefinition>();

        private bool IsUseless(FeatureDefinition upgrade, GridCoord coord) =>
            upgrade.OnlyFeedsItsTile && !_catalog.TakesPartInHunts(upgrade.Id) && IsFertile(coord);

        private bool IsFertile(GridCoord coord) => _fertility.CanHold(coord) && _fertility.Get(coord) >= _fertility.Max;

        public bool CanApply(FeatureDefinition upgrade, GridCoord coord) =>
            !_peaks.IsPeak(coord) && !_locks.IsLocked(coord) && AvailableFor(coord).Contains(upgrade) && _creation.CanAfford(upgrade.Cost);

        public bool TryApply(FeatureDefinition upgrade, GridCoord coord)
        {
            if (!CanApply(upgrade, coord) || !_creation.TrySpend(upgrade.Cost)) return false;

            if (_features.IsWild(coord)) _features.Replace(coord, upgrade.Id);
            else _features.Add(coord, upgrade.Id);
            Applied?.Invoke(coord, upgrade);
            return true;
        }
    }
}
