using System;

namespace Godland.Game.Writer.Actions
{
    public sealed class HintSettings
    {
        public event Action Changed;

        public bool ShowHints { get; private set; }

        public void Toggle()
        {
            ShowHints = !ShowHints;
            Changed?.Invoke();
        }
    }
}
