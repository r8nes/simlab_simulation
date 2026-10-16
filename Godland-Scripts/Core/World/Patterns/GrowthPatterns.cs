using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Grid;
using Godland.Core.Randomness;

namespace Godland.Core.World.Patterns
{
    public enum PatternStyle
    {
        Random,
        Spots,
        Tongues,
        Maze
    }

    public sealed class GrowthPatterns
    {
        private const int Steps = 12;
        private const double DiffusionU = 0.16;
        private const double DiffusionV = 0.08;

        private readonly WorldGrid _grid;
        private readonly SeededRandom _random;
        private readonly Dictionary<string, (double[] U, double[] V)> _fields = new();

        public GrowthPatterns(WorldGrid grid, SeededRandom random)
        {
            _grid = grid;
            _random = random;
        }

        public IEnumerable<GridCoord> Rank(string channel, PatternStyle style, IEnumerable<GridCoord> seeds, IEnumerable<GridCoord> candidates)
        {
            var options = candidates.OrderBy(cell => cell.X).ThenBy(cell => cell.Y).ToList();
            if (style == PatternStyle.Random || options.Count < 2)
                return options.OrderBy(_ => _random.Next(100000)).ToList();

            var (u, v) = FieldOf(channel);
            foreach (var seed in seeds)
            {
                if (!_grid.InBounds(seed)) continue;

                var index = IndexOf(seed);
                u[index] = 0.5;
                v[index] = 0.25;
            }

            var (feed, kill) = style switch
            {
                PatternStyle.Spots => (0.035, 0.065),
                PatternStyle.Maze => (0.029, 0.057),
                _ => (0.037, 0.06)
            };
            for (var step = 0; step < Steps; step++) Diffuse(u, v, feed, kill);

            return options.OrderByDescending(cell => v[IndexOf(cell)]).ThenBy(_ => _random.Next(100)).ToList();
        }

        private (double[] U, double[] V) FieldOf(string channel)
        {
            if (_fields.TryGetValue(channel, out var field)) return field;

            var size = _grid.Width * _grid.Height;
            var u = new double[size];
            var v = new double[size];
            for (var i = 0; i < size; i++)
            {
                u[i] = 1.0;
                v[i] = _random.Next(100) / 2000.0;
            }

            _fields[channel] = (u, v);
            return (u, v);
        }

        private void Diffuse(double[] u, double[] v, double feed, double kill)
        {
            var width = _grid.Width;
            var height = _grid.Height;
            var nextU = new double[u.Length];
            var nextV = new double[v.Length];
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                var i = y * width + x;
                var reaction = u[i] * v[i] * v[i];
                nextU[i] = Math.Clamp(u[i] + DiffusionU * Laplacian(u, x, y) - reaction + feed * (1 - u[i]), 0, 1);
                nextV[i] = Math.Clamp(v[i] + DiffusionV * Laplacian(v, x, y) + reaction - (feed + kill) * v[i], 0, 1);
            }

            Array.Copy(nextU, u, u.Length);
            Array.Copy(nextV, v, v.Length);
        }

        private double Laplacian(double[] field, int x, int y)
        {
            var width = _grid.Width;
            var height = _grid.Height;
            var center = field[y * width + x];
            var sum = 0.0;
            sum += x > 0 ? field[y * width + x - 1] : center;
            sum += x < width - 1 ? field[y * width + x + 1] : center;
            sum += y > 0 ? field[(y - 1) * width + x] : center;
            sum += y < height - 1 ? field[(y + 1) * width + x] : center;
            return sum - 4 * center;
        }

        private int IndexOf(GridCoord cell) => cell.Y * _grid.Width + cell.X;
    }
}
