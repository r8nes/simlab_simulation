using System;
using UnityEngine;

namespace Godland.Game.Presentation.History
{
    public sealed class ChronicleView : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Transform _list;
        [SerializeField] private ChronicleRowView _rowPrefab;

        public bool IsShown => _root.activeSelf;

        public void Show(bool shown) => _root.SetActive(shown);

        public void Add(string text, Action onClick)
        {
            var row = Instantiate(_rowPrefab, _list);
            row.transform.SetAsFirstSibling();
            row.Bind(text, onClick);
        }
    }
}
