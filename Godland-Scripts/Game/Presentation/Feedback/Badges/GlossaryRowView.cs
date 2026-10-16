using TMPro;
using UnityEngine;

namespace Godland.Game.Presentation.Feedback.Badges
{
    public sealed class GlossaryRowView : MonoBehaviour
    {
        [SerializeField] private BadgeView _badge;
        [SerializeField] private TMP_Text _label;

        public void Bind(BadgeDefinition definition)
        {
            _badge.Bind(definition);
            _label.text = definition.Label;
        }
    }
}
