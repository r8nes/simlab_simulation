namespace Godland.Game.Presentation.Journal
{
    public readonly struct JournalEntry
    {
        public JournalEntry(int year, string text, JournalTone tone, int repeats = 1)
        {
            Repeats = repeats;
            Year = year;
            Text = text;
            Tone = tone;
        }

        public int Year { get; }
        public string Text { get; }
        public JournalTone Tone { get; }
        public int Repeats { get; }

        public string Display => Repeats > 1 ? $"{Text} (×{Repeats})" : Text;
    }
}
