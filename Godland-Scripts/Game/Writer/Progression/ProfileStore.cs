using System.IO;
using System.Linq;
using UnityEngine;

namespace Godland.Game.Writer.Progression
{
    public sealed class ProfileStore
    {
        private static string FilePath => Path.Combine(Application.persistentDataPath, "profile.json");

        public ProfileStore()
        {
            Profile = Load();
            Profile.Migrate();
        }

        public PlayerProfile Profile { get; }

        public void Save() => File.WriteAllText(FilePath, JsonUtility.ToJson(Profile, true));

        private static PlayerProfile Load() =>
            File.Exists(FilePath) ? JsonUtility.FromJson<PlayerProfile>(File.ReadAllText(FilePath)) ?? new PlayerProfile() : new PlayerProfile();
    }

    public sealed class FlowDirector : VContainer.Unity.IInitializable, System.IDisposable
    {
        private readonly ProfileStore _store;
        private readonly ProgressionConfig _config;
        private readonly Godland.Core.Flow.FlowBias _flow;
        private bool _recording;

        public FlowDirector(ProfileStore store, ProgressionConfig config, Godland.Core.Flow.FlowBias flow)
        {
            _store = store;
            _config = config;
            _flow = flow;
        }

        public void Initialize()
        {
            if (!_config.FlowEnabled) return;

            var profile = _store.Profile;
            var rules = _config.FlowRules;
            if (profile.PendingRun.Count > 0)
            {
                profile.FlowRuns++;
                Godland.Core.Flow.FlowStatistics.Fold(profile.Flow, profile.FlowRuns, profile.PendingRun, rules);
                profile.PendingRun.Clear();
            }

            var (percents, unseen) = Godland.Core.Flow.FlowStatistics.Bias(profile.Flow, profile.FlowRuns, rules);
            _flow.Load(percents, unseen);
            _flow.Noted += OnNoted;
            _recording = true;
            _store.Save();
        }

        public void Dispose()
        {
            if (!_recording) return;

            _flow.Noted -= OnNoted;
            _store.Save();
        }

        private void OnNoted(string key)
        {
            _store.Profile.PendingRun.Add(key);
            _store.Save();
        }
    }
}
