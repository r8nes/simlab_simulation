using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Godland.Game.Presentation.Feedback.Badges
{
    public sealed class BadgeGlossaryView : MonoBehaviour
    {
        [SerializeField] private Button _header;
        [SerializeField] private TMP_Text _headerLabel;
        [SerializeField] private Transform _list;
        [SerializeField] private GlossaryRowView _rowPrefab;

        public void Bind(Action onToggle) => _header.onClick.AddListener(() => onToggle());

        public void Fill(IReadOnlyList<BadgeDefinition> badges)
        {
            foreach (var badge in badges)
                Instantiate(_rowPrefab, _list).Bind(badge);
        }

        public void SetExpanded(bool expanded)
        {
            _list.gameObject.SetActive(expanded);
            _headerLabel.text = expanded ? "Значки  –" : "Значки  +";
        }
    }
}
