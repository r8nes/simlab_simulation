using System;
using Godland.Game.Writer.Run;

namespace Godland.Game.World.Tiles
{
    public sealed class TileBrush
    {
        private readonly RunConfig _config;
        private int _index;

        public TileBrush(RunConfig config)
        {
            _config = config;
        }

        public event Action Changed;

        public TileDefinition Current => _config.PlaceableTiles[_index];

        public void Next()
        {
            _index = (_index + 1) % _config.PlaceableTiles.Count;
            Changed?.Invoke();
        }
    }
}
