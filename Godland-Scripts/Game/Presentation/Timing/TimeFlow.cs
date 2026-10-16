using System;

namespace Godland.Game.Presentation.Timing
{
    public sealed class TimeFlow
    {
        private readonly TimeConfig _config;

        public TimeFlow(TimeConfig config)
        {
            _config = config;
        }

        public event Action Changed;

        public bool IsPaused { get; private set; }
        public int SpeedIndex { get; private set; }
        public float Speed => _config.Speeds[SpeedIndex];
        private int _blocks;

        public bool IsBlocked => _blocks > 0;
        public float Multiplier => IsPaused || IsBlocked ? 0f : Speed;

        public void Block()
        {
            _blocks++;
            Changed?.Invoke();
        }

        public void Unblock()
        {
            if (_blocks == 0) return;

            _blocks--;
            Changed?.Invoke();
        }

        public void TogglePause()
        {
            IsPaused = !IsPaused;
            Changed?.Invoke();
        }

        public void SetSpeed(int index)
        {
            if (index < 0 || index >= _config.Speeds.Length) return;

            SpeedIndex = index;
            IsPaused = false;
            Changed?.Invoke();
        }
    }
}
