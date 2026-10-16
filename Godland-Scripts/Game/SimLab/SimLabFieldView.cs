using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Godland.Game.SimLab
{
    public sealed class SimLabFieldView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField] private TMP_InputField _input;
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Dropdown _dropdown;

        public void ShowField(string label, string value, Action<string> changed)
        {
            _label.text = label;
            _input.gameObject.SetActive(true);
            _dropdown.gameObject.SetActive(false);
            _input.text = value;
            _input.onEndEdit.AddListener(text => changed(text));
            _button.interactable = false;
        }

        public void ShowHeader(string label, Action clicked)
        {
            _label.text = $"<b><color=#ffd27a>{label}</color></b>";
            _input.gameObject.SetActive(false);
            _dropdown.gameObject.SetActive(false);
            _button.onClick.AddListener(() => clicked());
        }

        public void ShowChoice(string label, IReadOnlyList<string> options, int selected, Action<int> changed)
        {
            _label.text = label;
            _input.gameObject.SetActive(false);
            _dropdown.gameObject.SetActive(true);
            _dropdown.ClearOptions();
            _dropdown.AddOptions(new List<string>(options));
            _dropdown.SetValueWithoutNotify(selected);
            _dropdown.onValueChanged.AddListener(index => changed(index));
            _button.interactable = false;
        }

        public void SetValue(string value) => _input.SetTextWithoutNotify(value);
    }
}
