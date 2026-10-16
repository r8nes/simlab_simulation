using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

namespace Godland.Game.Presentation.Feedback.Badges
{
    public sealed class TileBadgesView : MonoBehaviour
    {
        [SerializeField] private Transform _row;
        [SerializeField] private BadgeView _badgePrefab;

        private readonly List<BadgeView> _badges = new();
        private Vector3? _baseScale;

        public void Show(Vector3 position, IReadOnlyList<BadgeDefinition> definitions)
        {
            transform.position = position;
            if (!gameObject.activeSelf)
            {
                _baseScale ??= transform.localScale;
                gameObject.SetActive(true);
                Tween.Scale(transform, Vector3.zero, _baseScale.Value, 0.2f, Ease.OutBack);
            }

            while (_badges.Count < definitions.Count)
                _badges.Add(Instantiate(_badgePrefab, _row));

            for (var i = 0; i < _badges.Count; i++)
            {
                var visible = i < definitions.Count;
                if (_badges[i].gameObject.activeSelf != visible) _badges[i].gameObject.SetActive(visible);
                if (visible) _badges[i].Bind(definitions[i]);
            }
        }

        public void Hide()
        {
            if (gameObject.activeSelf) gameObject.SetActive(false);
        }

        public void Face(Quaternion rotation) => transform.rotation = rotation;
    }
}
