using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Godland.Game.Presentation.Menu
{
    public sealed class SceneLoader
    {
        private readonly MenuSettings _settings;

        public SceneLoader(MenuSettings settings) => _settings = settings;

        public UniTask LoadGame() => SceneManager.LoadSceneAsync(_settings.GameScene).ToUniTask();

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
