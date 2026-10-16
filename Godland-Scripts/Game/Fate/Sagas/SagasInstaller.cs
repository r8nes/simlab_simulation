using Godland.Core;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.Fate.Sagas
{
    public sealed class SagasInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<SagaPresenter>();
        }
    }
}
