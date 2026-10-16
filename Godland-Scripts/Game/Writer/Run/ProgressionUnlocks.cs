using System.Linq;
using Godland.Game.Writer.Commandments;
using Godland.Game.Writer.Progression;
using VContainer;

namespace Godland.Game.Writer.Run
{
    public sealed class ProgressionUnlocks
    {
        private readonly ProgressionConfig _config;
        private readonly PlayerProfile _profile;

        [Inject]
        public ProgressionUnlocks(ProgressionConfig config, ProfileStore store)
        {
            _config = config;
            _profile = store.Profile;
        }

        private ProgressionUnlocks()
        {
        }

        public static ProgressionUnlocks Everything { get; } = new();

        public static string KeyOf(ProgressionFeature feature) => $"feature:{feature}";

        public static string KeyOf(CommandmentWord word) => $"word:{word.name}";

        public bool IsUnlocked(ProgressionFeature feature) =>
            _profile == null || feature == ProgressionFeature.None || _config.StartingFeatures.Contains(feature) || _profile.HasUnlocked(KeyOf(feature));

        public bool IsUnlocked(CommandmentWord word) => _profile == null || !word.RequiresUnlock || _profile.HasUnlocked(KeyOf(word));
    }
}
