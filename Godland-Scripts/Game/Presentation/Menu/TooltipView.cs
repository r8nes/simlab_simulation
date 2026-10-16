using TMPro;
using UnityEngine;

namespace Godland.Game.Presentation.Menu
{
    public sealed class TooltipView : MonoBehaviour
    {
        [SerializeField] private RectTransform _panel;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Vector2 _offset = new(24f, -24f);

        public void Show(string text, Vector3 position)
        {
            _text.text = text;
            _panel.position = position + (Vector3)_offset;
            _panel.gameObject.SetActive(true);
        }

        public void Hide() => _panel.gameObject.SetActive(false);
    }
}
