using System;
using System.Collections.Generic;
using Godland.Core.World.Cells;
using Godland.Core.World.Grid;
using VContainer.Unity;

namespace Godland.Game.Presentation.Feedback
{
    public sealed class FertilityPopupPresenter : IStartable, IDisposable
    {
        private readonly CellFertility _fertility;
        private readonly FloatingTextSpawner _spawner;
        private readonly Dictionary<GridCoord, int> _shown = new();

        public FertilityPopupPresenter(CellFertility fertility, FloatingTextSpawner spawner)
        {
            _fertility = fertility;
            _spawner = spawner;
        }

        public void Start() => _fertility.Changed += Show;

        public void Dispose() => _fertility.Changed -= Show;

        private void Show(GridCoord coord, int value)
        {
            var gain = value - _shown.GetValueOrDefault(coord);
            _shown[coord] = value;
            if (gain > 0) _spawner.Spawn(coord, $"+{gain}", FloatingTextStyle.Fertility);
        }
    }
}
