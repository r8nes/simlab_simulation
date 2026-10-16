using UnityEngine;
using UnityEngine.EventSystems;

namespace Godland.Game.Presentation.Overview
{
    public sealed class PanelResizeHandle : MonoBehaviour, IDragHandler
    {
        [SerializeField] private RectTransform _target;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private Vector2 _minSize = new(360f, 260f);
        [SerializeField] private Vector2 _maxSize = new(900f, 1000f);

        public void OnDrag(PointerEventData eventData)
        {
            var delta = eventData.delta / _canvas.scaleFactor;
            var rect = _target.rect;
            var width = Mathf.Clamp(rect.width + delta.x, _minSize.x, _maxSize.x);
            var height = Mathf.Clamp(rect.height - delta.y, _minSize.y, _maxSize.y);
            _target.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            _target.offsetMin = new Vector2(_target.offsetMin.x, _target.offsetMin.y - (height - rect.height));
        }
    }
}
