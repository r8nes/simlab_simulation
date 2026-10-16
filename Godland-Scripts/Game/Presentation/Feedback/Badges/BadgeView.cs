using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Godland.Game.Presentation.Feedback.Badges
{
    public sealed class BadgeView : MonoBehaviour
    {
        [SerializeField] private Image _circle;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _glyph;

        private BadgeDefinition _bound;

        public void Bind(BadgeDefinition definition)
        {
            if (_bound == definition) return;

            _bound = definition;
            _circle.color = definition.Color;
            var hasIcon = definition.Icon != null;
            _icon.gameObject.SetActive(hasIcon);
            _icon.sprite = definition.Icon;
            _glyph.gameObject.SetActive(!hasIcon);
            _glyph.text = definition.Glyph;
        }
    }
}
