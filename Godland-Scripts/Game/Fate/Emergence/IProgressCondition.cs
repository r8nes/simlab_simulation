namespace Godland.Game.Fate.Emergence
{
    public interface IProgressCondition
    {
        (int Value, int Threshold) Progress(EmergenceContext context);
    }
}
