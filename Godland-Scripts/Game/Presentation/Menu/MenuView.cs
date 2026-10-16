using System;
using UnityEngine;
using UnityEngine.UI;

namespace Godland.Game.Presentation.Menu
{
    public sealed class MenuView : MonoBehaviour
    {
        [SerializeField] private Button _newWorld;
        [SerializeField] private Button _quit;

        public event Action NewWorldClicked;
        public event Action QuitClicked;

        public void Bind()
        {
            _newWorld.onClick.AddListener(() => NewWorldClicked?.Invoke());
            _quit.onClick.AddListener(() => QuitClicked?.Invoke());
        }

        public void Unbind()
        {
            _newWorld.onClick.RemoveAllListeners();
            _quit.onClick.RemoveAllListeners();
        }

        public void SetInteractable(bool interactable)
        {
            _newWorld.interactable = interactable;
            _quit.interactable = interactable;
        }
    }
}
