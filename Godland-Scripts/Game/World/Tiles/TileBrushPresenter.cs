using System;
using Godland.Core.World.Creation;
using Godland.Core.World.Grid;
using VContainer.Unity;

namespace Godland.Game.World.Tiles
{
    public sealed class TileBrushPresenter : IStartable, IDisposable
    {
        private readonly TileBrush _brush;
        private readonly CellPurchase _purchase;
        private readonly TileBrushView _view;

        public TileBrushPresenter(TileBrush brush, CellPurchase purchase, TileBrushView view)
        {
            _brush = brush;
            _purchase = purchase;
            _view = view;
        }

        public void Start()
        {
            Show();
            _brush.Changed += Show;
            _purchase.Purchased += OnPurchased;
        }

        public void Dispose()
        {
            _brush.Changed -= Show;
            _purchase.Purchased -= OnPurchased;
        }

        private void OnPurchased(GridCoord _) => Show();

        private void Show() => _view.Show(_brush.Current.DisplayName, _brush.Current.Cost, _purchase.Cost);
    }
}
