using TMPro;
using UnityEngine;

namespace Godland.Game.Presentation.Resources
{
    public abstract class ResourceCounterView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;

        protected abstract string Caption { get; }

        public void Show(int amount) => _label.text = $"{Caption}: {amount}";
    }
}
