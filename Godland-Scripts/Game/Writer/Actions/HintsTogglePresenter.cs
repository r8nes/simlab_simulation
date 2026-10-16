using System;
using VContainer.Unity;

namespace Godland.Game.Writer.Actions
{
    public sealed class HintsTogglePresenter : IStartable, IDisposable
    {
        private readonly HintSettings _settings;
        private readonly HintsToggleView _view;

        public HintsTogglePresenter(HintSettings settings, HintsToggleView view)
        {
            _settings = settings;
            _view = view;
        }

        public void Start()
        {
            _view.Bind(_settings.Toggle);
            _view.Show(_settings.ShowHints);
            _settings.Changed += OnChanged;
        }

        public void Dispose() => _settings.Changed -= OnChanged;

        private void OnChanged() => _view.Show(_settings.ShowHints);
    }
}
