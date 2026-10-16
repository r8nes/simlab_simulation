using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Godland.Game.Writer.Actions
{
    public sealed class TileActionsView : MonoBehaviour
    {
        [SerializeField] private Transform _container;
        [SerializeField] private ActionButtonView _buttonPrefab;
        [SerializeField] private TMP_Text _headerPrefab;

        private readonly List<ActionButtonView> _buttons = new();
        private readonly List<TMP_Text> _headers = new();

        public void Show(IReadOnlyList<ActionSection> sections)
        {
            var sibling = 0;
            var buttonIndex = 0;
            var headerIndex = 0;

            foreach (var section in sections)
            {
                if (section.Entries.Count == 0) continue;

                var header = Take(_headers, _headerPrefab, headerIndex++);
                if (header.text != section.Title) header.text = section.Title;
                Place(header.transform, sibling++);

                foreach (var entry in section.Entries)
                {
                    var button = Take(_buttons, _buttonPrefab, buttonIndex++);
                    button.Bind(entry);
                    Place(button.transform, sibling++);
                }
            }

            HideFrom(_headers, headerIndex);
            HideFrom(_buttons, buttonIndex);
        }

        private T Take<T>(List<T> pool, T prefab, int index) where T : Component
        {
            if (index == pool.Count) pool.Add(Instantiate(prefab, _container));

            var item = pool[index];
            if (!item.gameObject.activeSelf) item.gameObject.SetActive(true);
            return item;
        }

        private static void Place(Transform item, int index)
        {
            if (item.GetSiblingIndex() != index) item.SetSiblingIndex(index);
        }

        private static void HideFrom<T>(List<T> pool, int start) where T : Component
        {
            for (var i = start; i < pool.Count; i++)
                if (pool[i].gameObject.activeSelf)
                    pool[i].gameObject.SetActive(false);
        }
    }
}
