using Godland.Core.World.Grid;

namespace Godland.Game.Presentation.History
{
    public readonly struct ChronicleEntry
    {
        public ChronicleEntry(int year, string text, GridCoord? place)
        {
            Year = year;
            Text = text;
            Place = place;
        }

        public int Year { get; }
        public string Text { get; }
        public GridCoord? Place { get; }
    }
}
