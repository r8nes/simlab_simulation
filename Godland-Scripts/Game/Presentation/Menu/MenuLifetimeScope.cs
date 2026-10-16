using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.Presentation.Menu
{
    public sealed class MenuLifetimeScope : LifetimeScope
    {
        [SerializeField, Required, AssetsOnly] private MenuSettings _settings;

        protected override void Configure(IContainerBuilder builder)
        {
            var game = _settings.Game;
            builder.RegisterInstance(_settings);
            builder.RegisterInstance(game.TileCatalog);
            builder.RegisterInstance(game.FeatureCatalog);
            builder.RegisterInstance(game.Settlements);
            builder.RegisterInstance(game.Titans);
            builder.RegisterInstance(game.TileAnimation);
            builder.RegisterInstance(game.Progression);
            builder.Register<Godland.Game.Writer.Progression.ProfileStore>(Lifetime.Singleton);
            builder.Register<SceneLoader>(Lifetime.Singleton);
            builder.RegisterComponentInHierarchy<MenuView>();
            builder.RegisterComponentInHierarchy<MenuShowcaseView>();
            builder.RegisterComponentInHierarchy<ChronicleBookView>();
            builder.RegisterComponentInHierarchy<TooltipView>();
            builder.RegisterEntryPoint<MenuPresenter>();
            builder.RegisterEntryPoint<MenuShowcasePresenter>();
            builder.RegisterEntryPoint<ChronicleBookPresenter>();
        }
    }
}
