using System;
using Godland.Core.World.Creation;
using Godland.Core.World.Grid;
using Godland.Game.World.Evolution;
using Godland.Game.World.Tiles;
using VContainer.Unity;

namespace Godland.Game.Presentation.Feedback
{
    public sealed class RewardPopupPresenter : IStartable, IDisposable
    {
        private readonly ProsperityRewards _rewards;
        private readonly CreationYield _yield;
        private readonly FloatingTextSpawner _spawner;

        public RewardPopupPresenter(ProsperityRewards rewards, CreationYield creationYield, FloatingTextSpawner spawner)
        {
            _rewards = rewards;
            _yield = creationYield;
            _spawner = spawner;
        }

        public void Start()
        {
            _rewards.Rewarded += Show;
            _yield.Yielded += Show;
        }

        public void Dispose()
        {
            _rewards.Rewarded -= Show;
            _yield.Yielded -= Show;
        }

        private void Show(GridCoord coord, int amount) => _spawner.Spawn(coord, $"+{amount} созидания", FloatingTextStyle.Reward);
    }
}
