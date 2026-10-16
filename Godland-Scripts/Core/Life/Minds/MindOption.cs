namespace Godland.Core.Life.Minds
{
    public readonly struct MindOption
    {
        public MindOption(string id, int nature)
        {
            Id = id;
            Nature = nature;
        }

        public string Id { get; }
        public int Nature { get; }
        public bool IsAllowed => Nature > 0;
    }
}
