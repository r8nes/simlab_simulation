namespace Godland.Game.Writer.Gifts
{
    public interface IGiftEffect
    {
        string Describe();
        bool IsRelevant(GiftContext context);
        void Apply(GiftContext context);
    }
}
