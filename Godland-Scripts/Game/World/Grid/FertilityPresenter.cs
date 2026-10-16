using System;
using Godland.Core.World.Cells;
using Godland.Core.World.Grid;
using UnityEngine;
using VContainer.Unity;

namespace Godland.Game.World.Grid
{
    public sealed class FertilityPresenter : IStartable, IDisposable
    {
        private readonly WorldGrid _grid;
        private readonly CellFertility _fertility;
        private readonly GridView _view;

        public FertilityPresenter(WorldGrid grid, CellFertility fertility, GridView view)
        {
            _grid = grid;
            _fertility = fertility;
            _view = view;
        }

        public void Start()
        {
            foreach (var coord in _grid.Coords)
                Show(coord, _fertility.Get(coord));

            _fertility.Changed += Show;
        }

        public void Dispose() => _fertility.Changed -= Show;

        private void Show(GridCoord coord, int fertility) =>
            _view.SetFertility(coord, Mathf.Clamp01((float)fertility / _fertility.Max));
    }
}
