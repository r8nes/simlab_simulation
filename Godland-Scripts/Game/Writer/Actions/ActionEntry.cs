using System;

namespace Godland.Game.Writer.Actions
{
    public readonly struct ActionEntry
    {
        public ActionEntry(string label, bool interactable, Action onClick)
        {
            Label = label;
            Interactable = interactable;
            OnClick = onClick;
        }

        public string Label { get; }
        public bool Interactable { get; }
        public Action OnClick { get; }
    }
}
