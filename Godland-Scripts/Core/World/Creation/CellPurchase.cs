using System;
using Godland.Core.World.Grid;

namespace Godland.Core.World.Creation
{
    public sealed class CellPurchase
    {
        private readonly WorldGrid _grid;
        private readonly CreationPoints _creation;
        private readonly int _baseCost;
        private readonly int _costStep;
        private int _purchased;

        public CellPurchase(WorldGrid grid, CreationPoints creation, int baseCost, int costStep)
        {
            _grid = grid;
            _creation = creation;
            _baseCost = baseCost;
            _costStep = costStep;
        }

        public event Action<GridCoord> Purchased;

        public int Cost => _baseCost + _costStep * _purchased;

        public bool CanBuy(GridCoord coord) => _grid.IsPurchasable(coord) && _creation.CanAfford(Cost);

        public bool TryBuy(GridCoord coord)
        {
            if (!CanBuy(coord) || !_creation.TrySpend(Cost)) return false;

            _purchased++;
            _grid.Unlock(coord);
            Purchased?.Invoke(coord);
            return true;
        }
    }
}
