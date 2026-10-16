using Godland.Core;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.Writer.Commandments
{
    public sealed class CommandmentsInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<CommandmentView>();
            builder.RegisterEntryPoint<CommandmentPresenter>();
        }
    }
}
