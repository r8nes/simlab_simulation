using Godland.Game.Presentation.Inputs;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.Presentation.Cameras
{
    public sealed class CameraInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<PointerInput>().AsSelf();
            builder.RegisterComponentInHierarchy<CameraView>();
            builder.RegisterEntryPoint<CameraInput>().AsSelf();
            builder.RegisterEntryPoint<CameraController>().AsSelf();
        }
    }
}
