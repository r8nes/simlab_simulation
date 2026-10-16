using UnityEngine;
using UnityEngine.EventSystems;

namespace Godland.Game.Presentation.Overview
{
    public sealed class PanelDragHandle : MonoBehaviour, IDragHandler
    {
        private const float KeepVisible = 60f;

        [SerializeField] private RectTransform _target;
        [SerializeField] private Canvas _canvas;

        public void OnDrag(PointerEventData eventData)
        {
            _target.anchoredPosition += eventData.delta / _canvas.scaleFactor;
            Clamp();
        }

        private void Clamp()
        {
            var area = ((RectTransform)_canvas.transform).rect;
            var corners = new Vector3[4];
            _target.GetWorldCorners(corners);
            var canvas = (RectTransform)_canvas.transform;
            var min = canvas.InverseTransformPoint(corners[0]);
            var max = canvas.InverseTransformPoint(corners[2]);
            var shift = Vector2.zero;
            if (max.y > area.yMax) shift.y = area.yMax - max.y;
            if (max.y < area.yMin + KeepVisible) shift.y = area.yMin + KeepVisible - max.y;
            if (max.x < area.xMin + KeepVisible) shift.x = area.xMin + KeepVisible - max.x;
            if (min.x > area.xMax - KeepVisible) shift.x = area.xMax - KeepVisible - min.x;
            _target.anchoredPosition += shift;
        }
    }
}
