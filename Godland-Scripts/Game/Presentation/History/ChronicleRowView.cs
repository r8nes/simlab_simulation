using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Godland.Game.Presentation.History
{
    public sealed class ChronicleRowView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _label;

        public void Bind(string text, Action onClick)
        {
            _label.text = text;
            _button.interactable = onClick != null;
            if (onClick != null) _button.onClick.AddListener(() => onClick());
        }

        public void SetText(string text) => _label.text = text;

        public void Fit(float height, float fontSize)
        {
            var element = gameObject.AddComponent<LayoutElement>();
            element.minHeight = height;
            element.preferredHeight = height;
            _label.enableAutoSizing = false;
            _label.fontSize = fontSize;
        }
    }
}
