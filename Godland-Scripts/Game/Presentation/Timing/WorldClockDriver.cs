using Godland.Core;
using VContainer.Unity;

namespace Godland.Game.Presentation.Timing
{
    public sealed class WorldClockDriver : ITickable
    {
        private readonly WorldSimulation _simulation;
        private readonly TimeFlow _flow;
        private readonly TimeConfig _config;

        private float _elapsed;

        public WorldClockDriver(WorldSimulation simulation, TimeFlow flow, TimeConfig config)
        {
            _simulation = simulation;
            _flow = flow;
            _config = config;
        }

        public void Tick()
        {
            _elapsed += UnityEngine.Time.deltaTime * _flow.Multiplier;
            while (_elapsed >= _config.SecondsPerYear)
            {
                _elapsed -= _config.SecondsPerYear;
                _simulation.AdvanceYear();
            }
        }
    }
}
