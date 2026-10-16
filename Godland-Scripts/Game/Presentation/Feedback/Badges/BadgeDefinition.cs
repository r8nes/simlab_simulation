using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Presentation.Feedback.Badges
{
    [Serializable]
    public sealed class BadgeDefinition
    {
        [field: SerializeField, HorizontalGroup("Main", 130), HideLabel]
        public BadgeKind Kind { get; private set; }

        [field: SerializeField, HorizontalGroup("Main", 40), HideLabel, InfoBox("Символ, если нет иконки.")]
        public string Glyph { get; private set; } = "!";

        [field: SerializeField, HorizontalGroup("Main"), HideLabel]
        public Color Color { get; private set; } = Color.white;

        [field: SerializeField, PreviewField(40), InfoBox("Иконка вместо символа (необязательно).")]
        public Sprite Icon { get; private set; }

        [field: SerializeField]
        public string Label { get; private set; }
    }
}
