using System;
using System.Collections.Generic;
using Godland.Core.Powers.Gods;
using Godland.Core.World.Grid;
using Godland.Core.Life.People;
using Godland.Game.Fate.Epochs;
using Godland.Game.World.Grid;
using Godland.Game.Presentation.Journal;
using Godland.Game.Life.People;
using Godland.Game.World.Tiles;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.Powers.Gods
{
    public sealed class GodsPresenter : IStartable, IDisposable
    {
        private const float BannerSeconds = 4f;

        private readonly Pantheon _pantheon;
        private readonly GodsConfig _config;
        private readonly SettlementConfig _settlementConfig;
        private readonly EventJournal _journal;
        private readonly EpochView _banner;
        private readonly GridGeometry _geometry;
        private readonly TilesView _root;
        private readonly IObjectResolver _resolver;
        private readonly Dictionary<God, GodAvatarView> _avatars = new();

        public GodsPresenter(
            Pantheon pantheon, GodsConfig config, SettlementConfig settlementConfig, EventJournal journal, EpochView banner,
            GridGeometry geometry, TilesView root, IObjectResolver resolver)
        {
            _pantheon = pantheon;
            _config = config;
            _settlementConfig = settlementConfig;
            _journal = journal;
            _banner = banner;
            _geometry = geometry;
            _root = root;
            _resolver = resolver;
        }

        public void Start()
        {
            _pantheon.Born += OnBorn;
            _pantheon.Moved += OnMoved;
            _pantheon.Fallen += OnFallen;
            _pantheon.Acted += OnActed;
            _pantheon.IdolFaded += OnIdolFaded;
            _pantheon.IdolResisted += OnIdolResisted;
            _pantheon.Died += OnDied;
        }

        public void Dispose()
        {
            _pantheon.Born -= OnBorn;
            _pantheon.Moved -= OnMoved;
            _pantheon.Fallen -= OnFallen;
            _pantheon.Acted -= OnActed;
            _pantheon.IdolFaded -= OnIdolFaded;
            _pantheon.IdolResisted -= OnIdolResisted;
            _pantheon.Died -= OnDied;
        }

        private GodDefinition DefinitionOf(God god) => _config.DefinitionOf(god.Rule.Id);

        private void OnMoved(God god, Godland.Core.World.Grid.GridCoord from)
        {
            if (_avatars.TryGetValue(god, out var avatar))
                PrimeTween.Tween.Position(avatar.Body.transform, _geometry.ToWorld(god.Seat), 1f, PrimeTween.Ease.InOutSine);
            _journal.Write($"{_config.NameOf(god)} перенёс престол: прежняя земля больше не его", JournalTone.Info);
        }

        private void OnBorn(God god)
        {
            var definition = DefinitionOf(god);
            var avatar = _resolver.Instantiate(_config.AvatarPrefab, _root.Root);
            avatar.name = $"God {definition.Id}";
            avatar.Body.Place(_geometry.ToWorld(god.Seat), _geometry.CellSize);
            avatar.Body.Appear(0f);
            avatar.Tint(definition.Color);
            _avatars[god] = avatar;

            var name = _config.NameOf(god);
            _banner.Announce(god.HeroName != null ? "Герой вознёсся" : "Родился бог", name, BannerSeconds);
            _journal.Write(god.HeroName != null ? $"Герой стал богом: {name}" : $"Из стихий родился бог: {name}", JournalTone.Good);
        }

        private void OnFallen(God god)
        {
            if (_avatars.Remove(god, out var avatar)) avatar.Body.Disappear();
            _journal.Write($"Престол разрушен — бог пал: {_config.NameOf(god)}", JournalTone.Threat);
        }

        private void OnActed(God god, IReadOnlyList<GridCoord> cells)
        {
            if (cells.Count == 0) return;

            var definition = DefinitionOf(god);
            _journal.Write($"{god.HeroName ?? definition.DisplayName}: {definition.DeedName} (тайлов: {cells.Count})", JournalTone.Info);
        }

        private void OnDied(God god)
        {
            if (_avatars.Remove(god, out var avatar)) avatar.Body.Disappear();
            _journal.Write($"Бог умер от старости: {_config.NameOf(god)}. Скоро придёт наследник", JournalTone.Info);
        }

        private void OnIdolFaded(Religion religion) =>
            _journal.Write($"Идол угас: {IdolName(religion)}", JournalTone.Info);

        private void OnIdolResisted(Religion religion) =>
            _journal.Write($"Идол {IdolName(religion)} не сдаётся богам: его вера тверда", JournalTone.Threat);

        private string IdolName(Religion religion) =>
            religion.IdolId != null ? _settlementConfig.IdolById(religion.IdolId).DisplayName : "без имени";
    }
}
