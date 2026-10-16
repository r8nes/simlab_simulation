using System;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace Godland.Game.Presentation.Menu
{
    public sealed class MenuPresenter : IStartable, IDisposable
    {
        private readonly MenuView _view;
        private readonly SceneLoader _loader;

        public MenuPresenter(MenuView view, SceneLoader loader)
        {
            _view = view;
            _loader = loader;
        }

        public void Start()
        {
            _view.Bind();
            _view.NewWorldClicked += OnNewWorld;
            _view.QuitClicked += _loader.Quit;
        }

        public void Dispose()
        {
            _view.NewWorldClicked -= OnNewWorld;
            _view.QuitClicked -= _loader.Quit;
            _view.Unbind();
        }

        private void OnNewWorld()
        {
            _view.SetInteractable(false);
            _loader.LoadGame().Forget();
        }
    }
}
