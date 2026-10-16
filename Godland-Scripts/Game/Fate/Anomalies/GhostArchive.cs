using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godland.Core.Powers.Gods;
using Godland.Core.Life.Heroes;
using Godland.Core.World.Tiles;
using Godland.Core.World.Timing;
using Godland.Game.Powers.Gods;
using UnityEngine;
using VContainer.Unity;

namespace Godland.Game.Fate.Anomalies
{
    public sealed class GhostArchive : IPastWorld, IStartable, IDisposable
    {
        private const int SavePeriod = 100;
        private const string FileName = "ghosts.txt";

        private readonly WorldTiles _tiles;
        private readonly Pantheon _pantheon;
        private readonly HeroSaga _heroes;
        private readonly GodsConfig _gods;
        private readonly WorldClock _clock;
        private readonly List<string> _pastTiles = new();
        private readonly List<string> _pastNames = new();
        private readonly HashSet<string> _names = new();

        public GhostArchive(WorldTiles tiles, Pantheon pantheon, HeroSaga heroes, GodsConfig gods, WorldClock clock)
        {
            _tiles = tiles;
            _pantheon = pantheon;
            _heroes = heroes;
            _gods = gods;
            _clock = clock;
        }

        public IReadOnlyList<string> PastTiles => _pastTiles;
        public IReadOnlyList<string> PastNames => _pastNames;

        private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        public void Start()
        {
            if (File.Exists(FilePath))
            {
                var lines = File.ReadAllLines(FilePath);
                if (lines.Length >= 2)
                {
                    _pastTiles.AddRange(lines[0].Split(',', StringSplitOptions.RemoveEmptyEntries));
                    _pastNames.AddRange(lines[1].Split(',', StringSplitOptions.RemoveEmptyEntries));
                }
            }

            _pantheon.Born += OnGodBorn;
            _heroes.Born += OnHeroBorn;
            _clock.Ticked += OnTicked;
        }

        public void Dispose()
        {
            _pantheon.Born -= OnGodBorn;
            _heroes.Born -= OnHeroBorn;
            _clock.Ticked -= OnTicked;
        }

        private void OnGodBorn(God god) => _names.Add(_gods.NameOf(god));

        private void OnHeroBorn(Hero hero) => _names.Add(hero.Name);

        private void OnTicked(int year)
        {
            if (year % SavePeriod != 0) return;

            File.WriteAllLines(FilePath, new[] { string.Join(",", _tiles.All.Values), string.Join(",", _names) });
        }
    }
}
