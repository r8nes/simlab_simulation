using Godland.Core;
using Godland.Core.World.Timing;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.Presentation.Timing
{
    public sealed class TimeInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<TimeFlow>(Lifetime.Singleton);

            builder.RegisterComponentInHierarchy<ClockView>();
            builder.RegisterEntryPoint<WorldClockDriver>();
            builder.RegisterEntryPoint<TimeHotkeys>();
            builder.RegisterEntryPoint<ClockPresenter>();
        }
    }
}
