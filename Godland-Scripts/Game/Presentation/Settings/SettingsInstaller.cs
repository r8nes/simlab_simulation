using VContainer;
using VContainer.Unity;

namespace Godland.Game.Presentation.Settings
{
    public sealed class SettingsInstaller : IInstaller
    {
        private readonly GameSettings _settings;

        public SettingsInstaller(GameSettings settings)
        {
            _settings = settings;
        }

        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_settings);
            builder.RegisterInstance(_settings.Grid);
            builder.RegisterInstance(_settings.WorldRules);
            builder.RegisterInstance(_settings.Time);
            builder.RegisterInstance(_settings.Settlements).AsSelf().AsImplementedInterfaces();
            builder.RegisterInstance(_settings.Volcanoes).AsSelf().AsImplementedInterfaces();
            builder.RegisterInstance(_settings.Corruption).AsSelf().AsImplementedInterfaces();
            builder.RegisterInstance(_settings.Cities).AsSelf().AsImplementedInterfaces();
            builder.RegisterInstance(_settings.Overflow).AsSelf().AsImplementedInterfaces();
            builder.RegisterInstance(_settings.Kingdoms).AsSelf().AsImplementedInterfaces();
            builder.RegisterInstance(_settings.Heroes).AsSelf().AsImplementedInterfaces();
            builder.RegisterInstance(_settings.Babel).AsSelf().AsImplementedInterfaces();
            builder.RegisterInstance(_settings.Prophecies);
            builder.RegisterInstance(_settings.Sovereigns);
            builder.RegisterInstance(_settings.Anomalies);
            builder.RegisterInstance(_settings.Emergence);
            builder.RegisterInstance(_settings.Titans);
            builder.RegisterInstance(_settings.Races);
            builder.RegisterInstance(_settings.Catastrophes);
            builder.RegisterInstance(_settings.Reclaim);
            builder.RegisterInstance(_settings.Peaks).As<Godland.Core.World.Peaks.IPeakRules>();
            builder.RegisterInstance(_settings.Commandments);
            builder.RegisterInstance(_settings.Progression);
            builder.RegisterInstance(_settings.Sagas);
            builder.RegisterInstance(_settings.Commands);
            builder.RegisterInstance(_settings.Pantheons);
            builder.RegisterInstance(_settings.Dynasties);
            builder.RegisterInstance(_settings.Dilemmas);
            builder.RegisterInstance(_settings.Writer);
            builder.RegisterInstance(_settings.WildLand);
            builder.RegisterInstance(_settings.Minds).As<Godland.Core.Life.Minds.IMindRules>();
            builder.RegisterInstance(_settings.SuperBiomes).AsSelf().AsImplementedInterfaces();
            builder.RegisterInstance(_settings.Climate).AsSelf().AsImplementedInterfaces();
            builder.RegisterInstance(_settings.Run);
            builder.RegisterInstance(_settings.Epochs);
            builder.RegisterInstance(_settings.WorldEvents);
            builder.RegisterInstance(_settings.Gods).AsSelf().AsImplementedInterfaces();
            builder.RegisterInstance(_settings.TileCatalog).AsSelf().AsImplementedInterfaces();
            builder.RegisterInstance(_settings.FeatureCatalog).AsSelf().AsImplementedInterfaces();
            builder.RegisterInstance(_settings.Camera);
            builder.RegisterInstance(_settings.Feedback);
            builder.RegisterInstance(_settings.TileAnimation);
        }
    }
}
