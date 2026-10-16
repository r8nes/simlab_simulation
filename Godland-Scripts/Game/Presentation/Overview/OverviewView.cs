using System;
using System.Collections.Generic;
using Godland.Game.Presentation.History;
using TMPro;
using UnityEngine;

namespace Godland.Game.Presentation.Overview
{
    public sealed class OverviewView : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TMP_Text _header;
        [SerializeField] private Transform _tabs;
        [SerializeField] private Transform _list;
        [SerializeField] private ChronicleRowView _rowPrefab;
        [SerializeField] private ChronicleRowView _tabPrefab;
        [SerializeField] private UnityEngine.UI.GridLayoutGroup _tabGrid;
        [SerializeField] private int _tabColumns = 5;

        private readonly List<GameObject> _rows = new();

        public bool IsShown => _root.activeSelf;

        public void Show(bool shown) => _root.SetActive(shown);

        public ChronicleRowView AddTab(string title, Action onClick)
        {
            var tab = Instantiate(_tabPrefab, _tabs);
            tab.Bind(title, onClick);
            return tab;
        }

        public void FitTabs()
        {
            var width = ((RectTransform)_tabs).rect.width - _tabGrid.padding.horizontal - _tabGrid.spacing.x * (_tabColumns - 1);
            _tabGrid.cellSize = new Vector2(Mathf.Floor(width / _tabColumns), _tabGrid.cellSize.y);
        }

        public void ShowHeader(string text) => _header.text = text;

        public void ClearRows()
        {
            foreach (var row in _rows) Destroy(row);
            _rows.Clear();
        }

        public void AddCommand(string text, Action onClick)
        {
            var button = Instantiate(_tabPrefab, _list);
            button.Bind(text, onClick);
            button.Fit(40f, 19f);
            _rows.Add(button.gameObject);
        }

        public void AddRow(string text, Action onClick)
        {
            var row = Instantiate(_rowPrefab, _list);
            row.Bind(text, onClick);
            _rows.Add(row.gameObject);
        }
    }
}
