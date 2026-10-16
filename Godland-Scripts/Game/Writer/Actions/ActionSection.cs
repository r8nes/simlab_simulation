using System.Collections.Generic;

namespace Godland.Game.Writer.Actions
{
    public sealed class ActionSection
    {
        public ActionSection(string title)
        {
            Title = title;
        }

        public string Title { get; }
        public List<ActionEntry> Entries { get; } = new();
    }
}
