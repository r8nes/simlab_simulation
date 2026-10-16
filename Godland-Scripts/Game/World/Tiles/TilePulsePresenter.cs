using System;
using Godland.Core.World.Creation;
using Godland.Core.World.Grid;
using Godland.Game.Writer.Actions;
using VContainer.Unity;

namespace Godland.Game.World.Tiles
{
    public sealed class TilePulsePresenter : IStartable, IDisposable
    {
        private readonly TilesPresenter _tiles;
        private readonly TileActions _actions;
        private readonly ProsperityRewards _rewards;
        private readonly CreationYield _yield;

        public TilePulsePresenter(TilesPresenter tiles, TileActions actions, ProsperityRewards rewards, CreationYield creationYield)
        {
            _tiles = tiles;
            _actions = actions;
            _rewards = rewards;
            _yield = creationYield;
        }

        public void Start()
        {
            _actions.Applied += OnApplied;
            _rewards.Rewarded += OnRewarded;
            _yield.Yielded += OnRewarded;
        }

        public void Dispose()
        {
            _actions.Applied -= OnApplied;
            _rewards.Rewarded -= OnRewarded;
            _yield.Yielded -= OnRewarded;
        }

        private void OnApplied(GridCoord coord, TileActionDefinition _) => Pulse(coord);

        private void OnRewarded(GridCoord coord, int _) => Pulse(coord);

        private void Pulse(GridCoord coord)
        {
            if (_tiles.TryGetView(coord, out var view)) view.Pulse();
        }
    }
}
