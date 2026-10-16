namespace Godland.Game.Fate.Catastrophes
{
    public interface ICatastropheTrigger
    {
        bool IsReady(CatastropheContext context);
    }

    public interface ICatastropheStrike
    {
        void Strike(CatastropheContext context, string cause);
    }

    public interface IAgeLaw
    {
        void Begin(CatastropheContext context, string cause);
        void Year(CatastropheContext context, int age, string cause);
        void End(CatastropheContext context);
    }
}
