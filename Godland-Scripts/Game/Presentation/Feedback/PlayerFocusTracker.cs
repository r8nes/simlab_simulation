using System;
using Godland.Core.World.Creation;
using Godland.Core.World.Grid;
using Godland.Game.Writer.Actions;
using Godland.Game.World.Evolution;
using Godland.Game.Life.Features;
using Godland.Game.World.Tiles;
using VContainer.Unity;

namespace Godland.Game.Presentation.Feedback
{
    public sealed class PlayerFocusTracker : IInitializable, IDisposable
    {
        private readonly PlayerFocus _focus;
        private readonly TilePlacement _placement;
        private readonly CellPurchase _purchase;
        private readonly TileActions _actions;
        private readonly TileUpgrades _upgrades;
        private readonly TileEvolutions _evolutions;

        public PlayerFocusTracker(
            PlayerFocus focus, TilePlacement placement, CellPurchase purchase, TileActions actions,
            TileUpgrades upgrades, TileEvolutions evolutions)
        {
            _focus = focus;
            _placement = placement;
            _purchase = purchase;
            _actions = actions;
            _upgrades = upgrades;
            _evolutions = evolutions;
        }

        public void Initialize()
        {
            _placement.Placed += OnTile;
            _purchase.Purchased += _focus.Mark;
            _actions.Applied += OnAction;
            _upgrades.Applied += OnUpgrade;
            _evolutions.Evolved += OnTile;
        }

        public void Dispose()
        {
            _placement.Placed -= OnTile;
            _purchase.Purchased -= _focus.Mark;
            _actions.Applied -= OnAction;
            _upgrades.Applied -= OnUpgrade;
            _evolutions.Evolved -= OnTile;
        }

        private void OnTile(GridCoord coord, TileDefinition _) => _focus.Mark(coord);

        private void OnAction(GridCoord coord, TileActionDefinition _) => _focus.Mark(coord);

        private void OnUpgrade(GridCoord coord, FeatureDefinition _) => _focus.Mark(coord);
    }
}
