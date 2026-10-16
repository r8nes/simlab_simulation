namespace Godland.Core.Life.Kingdoms
{
    public sealed class War
    {
        public War(Kingdom first, Kingdom second, int endYear)
        {
            First = first;
            Second = second;
            EndYear = endYear;
        }

        public Kingdom First { get; }
        public Kingdom Second { get; }
        public int EndYear { get; }

        public bool Involves(Kingdom kingdom) => First == kingdom || Second == kingdom;

        public Kingdom EnemyOf(Kingdom kingdom) => First == kingdom ? Second : First;
    }
}
