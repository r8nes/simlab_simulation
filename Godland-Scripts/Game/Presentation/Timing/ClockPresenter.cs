using System;
using Godland.Core.World.Timing;
using VContainer.Unity;

namespace Godland.Game.Presentation.Timing
{
    public sealed class ClockPresenter : IStartable, IDisposable
    {
        private readonly WorldClock _clock;
        private readonly TimeFlow _flow;
        private readonly ClockView _view;

        public ClockPresenter(WorldClock clock, TimeFlow flow, ClockView view)
        {
            _clock = clock;
            _flow = flow;
            _view = view;
        }

        public void Start()
        {
            _view.ShowYear(_clock.Year);
            ShowSpeed();

            _clock.Ticked += _view.ShowYear;
            _flow.Changed += ShowSpeed;
        }

        public void Dispose()
        {
            _clock.Ticked -= _view.ShowYear;
            _flow.Changed -= ShowSpeed;
        }

        private void ShowSpeed() => _view.ShowSpeed(_flow.IsPaused, _flow.Speed);
    }
}
