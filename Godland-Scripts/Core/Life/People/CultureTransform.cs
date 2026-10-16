namespace Godland.Core.Life.People
{
    public sealed class CultureTransform
    {
        public CultureTransform(string fromTileId, string toTileId, int periodYears)
        {
            FromTileId = fromTileId;
            ToTileId = toTileId;
            PeriodYears = periodYears;
        }

        public string FromTileId { get; }
        public string ToTileId { get; }
        public int PeriodYears { get; }
    }
}
