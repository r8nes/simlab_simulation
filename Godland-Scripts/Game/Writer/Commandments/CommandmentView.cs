using System;
using System.Collections.Generic;
using Godland.Game.Presentation.History;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Godland.Game.Writer.Commandments
{
    public sealed class CommandmentView : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TMP_Text _sentence;
        [SerializeField] private Transform _when;
        [SerializeField] private Transform _then;
        [SerializeField] private Transform _where;
        [SerializeField] private Transform _amplifier;
        [SerializeField] private ChronicleRowView _optionPrefab;
        [SerializeField] private Button _confirm;

        private readonly List<GameObject> _options = new();

        public event Action Confirmed;

        public void Show(bool shown)
        {
            _root.SetActive(shown);
            _confirm.onClick.RemoveAllListeners();
            if (shown) _confirm.onClick.AddListener(() => Confirmed?.Invoke());
        }

        public void ShowSentence(string text) => _sentence.text = text;

        public void ClearOptions()
        {
            foreach (var option in _options) Destroy(option);
            _options.Clear();
        }

        public void AddOption(CommandmentSlot slot, string text, Action onClick)
        {
            var parent = slot switch
            {
                CommandmentSlot.When => _when,
                CommandmentSlot.Then => _then,
                CommandmentSlot.Where => _where,
                _ => _amplifier
            };
            var option = Instantiate(_optionPrefab, parent);
            option.Bind(text, onClick);
            option.Fit(54f, 22f);
            _options.Add(option.gameObject);
        }
    }
}
