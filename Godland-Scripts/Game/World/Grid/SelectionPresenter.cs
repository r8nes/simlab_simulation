using System;
using Godland.Core.World.Grid;
using Godland.Game.World.Tiles;
using VContainer.Unity;

namespace Godland.Game.World.Grid
{
    public sealed class SelectionPresenter : IStartable, IDisposable
    {
        private readonly TileSelection _selection;
        private readonly GridView _view;

        private GridCoord? _shown;

        public SelectionPresenter(TileSelection selection, GridView view)
        {
            _selection = selection;
            _view = view;
        }

        public void Start() => _selection.Changed += Show;

        public void Dispose() => _selection.Changed -= Show;

        private void Show()
        {
            if (_shown.HasValue) _view.SetSelected(_shown.Value, false);
            if (_selection.Selected.HasValue) _view.SetSelected(_selection.Selected.Value, true);
            _shown = _selection.Selected;
        }
    }
}
