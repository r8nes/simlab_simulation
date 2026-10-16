using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Godland.Game.Writer.Gifts
{
    public sealed class GiftCardView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _description;

        public void Bind(string title, string description, Action onClick)
        {
            _title.text = title;
            _description.text = description;
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => onClick());
        }
    }
}
