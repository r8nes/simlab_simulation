using System;
using Godland.Core.World.Grid;
using UnityEngine;

namespace Godland.Game.Presentation.Feedback
{
    public sealed class PlayerFocus
    {
        private const float WindowSeconds = 1f;

        private GridCoord? _coord;
        private float _time = float.NegativeInfinity;

        public void Mark(GridCoord coord)
        {
            _coord = coord;
            _time = Time.unscaledTime;
        }

        public bool Covers(GridCoord coord) =>
            _coord is { } focus
            && Time.unscaledTime - _time <= WindowSeconds
            && Math.Max(Math.Abs(coord.X - focus.X), Math.Abs(coord.Y - focus.Y)) <= 1;
    }
}
