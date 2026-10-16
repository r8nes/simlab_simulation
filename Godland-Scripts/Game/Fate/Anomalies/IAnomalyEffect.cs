namespace Godland.Game.Fate.Anomalies
{
    public interface IAnomalyEffect
    {
        bool CanBegin(AnomalyContext context);
        void Begin(AnomalyContext context, int year);
    }
}
