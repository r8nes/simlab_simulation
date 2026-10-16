using Godland.Core.World.Creation;
using Godland.Game.Presentation.Resources;
using Godland.Game.Presentation.Settings;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.World.Creation
{
    public sealed class CreationInstaller : IInstaller
    {
        private readonly GameSettings _settings;

        public CreationInstaller(GameSettings settings)
        {
            _settings = settings;
        }

        public void Install(IContainerBuilder builder)
        {

            builder.RegisterComponentInHierarchy<CreationPointsView>();
            builder.RegisterEntryPoint<ResourceCounterPresenter<CreationPoints, CreationPointsView>>();
        }
    }
}
