using System;
using System.Collections.Generic;
using Godland.Game.Presentation.History;
using TMPro;
using UnityEngine;

namespace Godland.Game.Writer.Dilemmas
{
    public sealed class DilemmaView : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Transform _options;
        [SerializeField] private ChronicleRowView _optionPrefab;

        private readonly List<GameObject> _rows = new();

        public void Show(string title, string text)
        {
            _root.SetActive(true);
            _title.text = title;
            _text.text = text;
            Clear();
        }

        public void Hide()
        {
            Clear();
            _root.SetActive(false);
        }

        public void AddOption(string text, Action onClick)
        {
            var row = Instantiate(_optionPrefab, _options);
            row.Bind(text, onClick);
            row.Fit(58f, 24f);
            _rows.Add(row.gameObject);
        }

        private void Clear()
        {
            foreach (var row in _rows) Destroy(row);
            _rows.Clear();
        }
    }
}
