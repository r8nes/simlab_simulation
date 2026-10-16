using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Godland.Game.Writer.Actions
{
    public sealed class ActionButtonView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _label;

        private Action _onClick;
        private bool _subscribed;

        public void Bind(ActionEntry entry)
        {
            if (!_subscribed)
            {
                _button.onClick.AddListener(() => _onClick?.Invoke());
                _subscribed = true;
            }

            _onClick = entry.OnClick;
            if (_label.text != entry.Label) _label.text = entry.Label;
            if (_button.interactable != entry.Interactable) _button.interactable = entry.Interactable;
        }
    }
}
