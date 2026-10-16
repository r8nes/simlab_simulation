using Godland.Core.World.Tiles;
using Godland.Core;
using System;
using Godland.Core.Randomness;
using Godland.Game.Presentation.Settings;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.Writer.Run
{
    public sealed class RunInstaller : IInstaller
    {
        private readonly GameSettings _settings;

        public RunInstaller(GameSettings settings)
        {
            _settings = settings;
        }

        public void Install(IContainerBuilder builder)
        {
            builder.Register<ProgressionUnlocks>(Lifetime.Singleton);
            builder.RegisterEntryPoint<RunStarter>();
        }

        public static int ResolveSeed(GameSettings settings)
        {
            var run = settings.Run;
            var seed = run.UseRandomSeed ? Environment.TickCount : run.Seed;
            Debug.Log($"Run seed: {seed}");
            return seed;
        }
    }
}
