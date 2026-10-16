using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Godland.Game.SimLab
{
    public sealed class SimLabView : MonoBehaviour
    {
        [SerializeField] private Transform _fields;
        [SerializeField] private SimLabFieldView _rowPrefab;
        [SerializeField] private Button _start;
        [SerializeField] private Button _stop;
        [SerializeField] private Button _reset;
        [SerializeField] private Button _download;
        [SerializeField] private TMP_Text _status;
        [SerializeField] private TMP_Text _log;
        [SerializeField] private ScrollRect _logScroll;

        private readonly List<GameObject> _rows = new();

        public event Action StartClicked;
        public event Action StopClicked;
        public event Action ResetClicked;
        public event Action DownloadClicked;

        public void Bind()
        {
            _start.onClick.AddListener(() => StartClicked?.Invoke());
            _stop.onClick.AddListener(() => StopClicked?.Invoke());
            _reset.onClick.AddListener(() => ResetClicked?.Invoke());
            _download.onClick.AddListener(() => DownloadClicked?.Invoke());
        }

        public void ClearFields()
        {
            foreach (var row in _rows) Destroy(row);
            _rows.Clear();
        }

        public SimLabFieldView AddRow()
        {
            var row = Instantiate(_rowPrefab, _fields);
            _rows.Add(row.gameObject);
            return row;
        }

        public void SetRunning(bool running)
        {
            _start.interactable = !running;
            _reset.interactable = !running;
            _stop.interactable = running;
        }

        public void ShowStatus(string text) => _status.text = text;

        public void ShowLog(string text)
        {
            _log.text = text;
            _logScroll.verticalNormalizedPosition = 0f;
        }
    }
}
