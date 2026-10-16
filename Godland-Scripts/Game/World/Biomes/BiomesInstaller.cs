using Godland.Core.World.Biomes;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.World.Biomes
{
    public sealed class BiomesInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<SuperBiomePresenter>();
        }
    }
}
