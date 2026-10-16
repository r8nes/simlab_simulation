using System;
using Godland.Core.World.Cells;
using Godland.Core.World.Grid;
using Godland.Game.Presentation.Feedback;
using Godland.Game.Writer.Run;
using Godland.Game.World.Tiles;
using VContainer.Unity;

namespace Godland.Game.World.Overflow
{
    public sealed class OverflowPresenter : IStartable, IDisposable
    {
        private readonly FertilityOverflow _overflow;
        private readonly OverflowConfig _config;
        private readonly ProgressionUnlocks _progression;
        private readonly FloatingTextSpawner _spawner;
        private readonly TileCatalog _catalog;

        public OverflowPresenter(
            FertilityOverflow overflow, OverflowConfig config, ProgressionUnlocks progression, FloatingTextSpawner spawner, TileCatalog catalog)
        {
            _overflow = overflow;
            _config = config;
            _progression = progression;
            _spawner = spawner;
            _catalog = catalog;
        }

        public void Start()
        {
            _overflow.SetEnabled(_progression.IsUnlocked(_config.RequiredFeature));
            _overflow.Transformed += OnTransformed;
        }

        public void Dispose() => _overflow.Transformed -= OnTransformed;

        private void OnTransformed(GridCoord coord, string tileId) =>
            _spawner.Spawn(coord, $"Избыток жизни: {_catalog.Get(tileId).DisplayName}", FloatingTextStyle.Evolution);
    }
}
