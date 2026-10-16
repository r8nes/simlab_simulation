using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Godland.Game.Presentation.Inspection
{
    public sealed class InspectToggleView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _label;

        public void Bind(Action onClick) => _button.onClick.AddListener(() => onClick());

        public void Show(bool inspecting) => _label.text = inspecting ? "Вернуться к миру [F]" : "Осмотреть тайл [F]";
    }
}
