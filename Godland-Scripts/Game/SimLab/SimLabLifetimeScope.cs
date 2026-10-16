using Godland.Game.Presentation.Settings;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.SimLab
{
    public sealed class SimLabLifetimeScope : LifetimeScope
    {
        [SerializeField] private GameSettings _settings;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_settings);
            builder.RegisterComponentInHierarchy<SimLabView>();
            builder.RegisterEntryPoint<SimLabPresenter>();
        }
    }
}
