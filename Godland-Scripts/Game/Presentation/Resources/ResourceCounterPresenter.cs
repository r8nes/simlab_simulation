using System;
using Godland.Core.Resources;
using VContainer.Unity;

namespace Godland.Game.Presentation.Resources
{
    public sealed class ResourceCounterPresenter<TPoints, TView> : IStartable, IDisposable
        where TPoints : ResourcePoints
        where TView : ResourceCounterView
    {
        private readonly TPoints _points;
        private readonly TView _view;

        public ResourceCounterPresenter(TPoints points, TView view)
        {
            _points = points;
            _view = view;
        }

        public void Start()
        {
            _view.Show(_points.Amount);
            _points.Changed += _view.Show;
        }

        public void Dispose() => _points.Changed -= _view.Show;
    }
}
