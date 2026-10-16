using Godland.Core.World.Cells;
using Godland.Core.World.Grid;
using Godland.Game.Presentation.Settings;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.World.Grid
{
    public sealed class GridInstaller : IInstaller
    {
        private readonly GameSettings _settings;

        public GridInstaller(GameSettings settings)
        {
            _settings = settings;
        }

        public void Install(IContainerBuilder builder)
        {
            var grid = _settings.Grid;
            builder.Register<GridGeometry>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<GridPointer>(Lifetime.Singleton);

            builder.RegisterComponentInHierarchy<GridView>();
            builder.RegisterEntryPoint<GridPresenter>();
            builder.RegisterEntryPoint<GridHoverPresenter>();
            builder.RegisterEntryPoint<SelectionPresenter>();
            builder.RegisterEntryPoint<FertilityPresenter>();
        }
    }
}
