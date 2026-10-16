using Godland.Core.Life.People;
using Godland.Game.Presentation.Resources;
using Godland.Game.Presentation.Feedback.Badges;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.Life.People
{
    public sealed class PeopleInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<DarkTimesPresenter>();
            builder.RegisterEntryPoint<FeudsPresenter>();

            builder.RegisterComponentInHierarchy<FaithView>();
            builder.RegisterEntryPoint<ResourceCounterPresenter<FaithPoints, FaithView>>();
            builder.RegisterEntryPoint<SettlementsPresenter>();
            builder.RegisterEntryPoint<ShrinesPresenter>();
            builder.Register<PeopleBadgeSource>(Lifetime.Singleton).As<ITileBadgeSource>();
        }
    }
}
