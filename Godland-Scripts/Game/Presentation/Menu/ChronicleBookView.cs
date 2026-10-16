using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Godland.Game.Presentation.Menu
{
    public sealed class ChronicleBookView : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _open;
        [SerializeField] private Button _close;
        [SerializeField] private TMP_Text _summary;
        [SerializeField] private Transform _rows;
        [SerializeField] private Transform _tabs;
        [SerializeField] private DiscoveryRowView _rowPrefab;

        public event Action Opened;
        public event Action Closed;

        public Transform Rows => _rows;
        public Transform Tabs => _tabs;
        public DiscoveryRowView RowPrefab => _rowPrefab;

        public void Bind()
        {
            _open.onClick.AddListener(() => Opened?.Invoke());
            _close.onClick.AddListener(() => Closed?.Invoke());
            _panel.SetActive(false);
        }

        public void Unbind()
        {
            _open.onClick.RemoveAllListeners();
            _close.onClick.RemoveAllListeners();
        }

        public void SetVisible(bool visible) => _panel.SetActive(visible);

        public void ShowSummary(string text) => _summary.text = text;
    }
}
