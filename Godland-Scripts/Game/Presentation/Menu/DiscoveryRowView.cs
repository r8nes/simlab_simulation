using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Godland.Game.Presentation.Menu
{
    public sealed class DiscoveryRowView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private Button _claim;
        [SerializeField] private TMP_Text _claimLabel;
        [SerializeField] private Image _background;
        [SerializeField] private Color _discovered = new(0.22f, 0.2f, 0.14f);
        [SerializeField] private Color _unknown = new(0.12f, 0.13f, 0.17f);
        [SerializeField] private Color _header = new(0.32f, 0.24f, 0.1f);
        [SerializeField] private Color _selected = new(0.5f, 0.38f, 0.14f);

        public event Action<DiscoveryRowView> Hovered;
        public event Action<DiscoveryRowView> Left;
        public event Action<DiscoveryRowView> Claimed;
        public event Action<DiscoveryRowView> Clicked;

        public string Tooltip { get; private set; }

        public void ShowHeader(string title)
        {
            _title.text = $"<b>{title}</b>";
            _title.alpha = 1f;
            _background.color = _header;
            Tooltip = null;
            ShowReward(null, false);
        }

        public void ShowTab(string title, bool selected)
        {
            _title.text = title;
            _title.alpha = 1f;
            _background.color = selected ? _selected : _header;
            Tooltip = null;
            ShowReward(null, false);
        }

        public void ShowReward(string label, bool claimable)
        {
            _claim.gameObject.SetActive(label != null);
            var title = (RectTransform)_title.transform;
            title.offsetMax = new Vector2(label == null ? -10f : -250f, title.offsetMax.y);
            if (label == null) return;

            _claimLabel.text = label;
            _claim.interactable = claimable;
            _claim.onClick.RemoveAllListeners();
            if (claimable) _claim.onClick.AddListener(() => Claimed?.Invoke(this));
        }

        public void Show(string title, string tooltip, bool discovered)
        {
            _title.text = title;
            _title.alpha = discovered ? 1f : 0.55f;
            _background.color = discovered ? _discovered : _unknown;
            Tooltip = tooltip;
        }

        public void OnPointerEnter(PointerEventData eventData) => Hovered?.Invoke(this);

        public void OnPointerExit(PointerEventData eventData) => Left?.Invoke(this);

        public void OnPointerClick(PointerEventData eventData) => Clicked?.Invoke(this);
    }
}
