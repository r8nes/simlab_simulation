using VContainer;
using VContainer.Unity;

namespace Godland.Game.Presentation.Journal
{
    public sealed class JournalInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<JournalView>();
            builder.RegisterEntryPoint<JournalPresenter>();
            builder.RegisterEntryPoint<RunLog>();
        }
    }
}
