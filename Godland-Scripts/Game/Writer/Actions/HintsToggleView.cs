using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Godland.Game.Writer.Actions
{
    public sealed class HintsToggleView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _label;

        public void Bind(Action onClick) => _button.onClick.AddListener(() => onClick());

        public void Show(bool enabled) => _label.text = enabled ? "Подсказки: вкл" : "Подсказки: выкл";
    }
}
