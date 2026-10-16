using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace Godland.Game.Presentation.Journal
{
    public sealed class JournalView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _lines;
        [SerializeField, Min(1)] private int _visibleEntries = 6;
        [SerializeField] private Color _info = new(0.8f, 0.82f, 0.88f);
        [SerializeField] private Color _good = new(0.6f, 0.85f, 1f);
        [SerializeField] private Color _threat = new(1f, 0.5f, 0.42f);

        private readonly StringBuilder _builder = new();

        public void Show(IReadOnlyList<JournalEntry> entries)
        {
            _builder.Clear();
            for (var i = Mathf.Max(0, entries.Count - _visibleEntries); i < entries.Count; i++)
            {
                var entry = entries[i];
                var color = ColorUtility.ToHtmlStringRGB(entry.Tone switch
                {
                    JournalTone.Good => _good,
                    JournalTone.Threat => _threat,
                    _ => _info
                });
                _builder.Append("<color=#888>").Append(entry.Year).Append("</color>  <color=#").Append(color).Append('>')
                    .Append(entry.Display).Append("</color>\n");
            }

            _lines.text = _builder.ToString();
        }
    }
}
