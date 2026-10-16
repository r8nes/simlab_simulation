using System;
using System.Collections.Generic;
using Godland.Core.Powers.Gods;
using Godland.Core.Powers.Titans;
using Godland.Game.Fate.Epochs;
using Godland.Game.Powers.Gods;
using Godland.Game.World.Grid;
using Godland.Game.Presentation.History;
using Godland.Game.Presentation.Journal;
using Godland.Game.World.Tiles;
using PrimeTween;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.Powers.Titans
{
    public sealed class TitansPresenter : IStartable, IDisposable
    {
        private const float BannerSeconds = 7f;
        private const float StepSeconds = 1.2f;

        private readonly TitanHost _host;
        private readonly TitanConfig _config;
        private readonly GodsConfig _gods;
        private readonly EventJournal _journal;
        private readonly EpochView _banner;
        private readonly Chronicle _chronicle;
        private readonly GridGeometry _geometry;
        private readonly TilesView _root;
        private readonly IObjectResolver _resolver;
        private readonly Dictionary<Titan, GodAvatarView> _avatars = new();

        public TitansPresenter(
            TitanHost host, TitanConfig config, GodsConfig gods, EventJournal journal, EpochView banner, Chronicle chronicle,
            GridGeometry geometry, TilesView root, IObjectResolver resolver)
        {
            _host = host;
            _config = config;
            _gods = gods;
            _journal = journal;
            _banner = banner;
            _chronicle = chronicle;
            _geometry = geometry;
            _root = root;
            _resolver = resolver;
        }

        public void Start()
        {
            _host.Awoke += OnAwoke;
            _host.Moved += OnMoved;
            _host.Devoured += OnDevoured;
            _host.Died += OnDied;
            _host.Slain += OnSlain;
            _host.Clashed += OnClashed;
            _host.Roused += OnRoused;
            _host.Tamed += OnTamed;
            _host.Pacted += OnPacted;
        }

        public void Dispose()
        {
            _host.Awoke -= OnAwoke;
            _host.Moved -= OnMoved;
            _host.Devoured -= OnDevoured;
            _host.Died -= OnDied;
            _host.Slain -= OnSlain;
            _host.Clashed -= OnClashed;
            _host.Roused -= OnRoused;
            _host.Tamed -= OnTamed;
            _host.Pacted -= OnPacted;
        }

        private void OnPacted(Titan titan, God god)
        {
            var text = $"Бог {_gods.NameOf(god)} договорился с титаном: {DefinitionOf(titan).DisplayName} щадит его верующих";
            _journal.Write(text, JournalTone.Info);
            _chronicle.Add(text, titan.Position);
        }

        private void OnClashed(Titan titan, God god, bool titanWon)
        {
            var text = titanWon
                ? $"{DefinitionOf(titan).DisplayName} одолел бога {_gods.NameOf(god)}"
                : $"Бог {_gods.NameOf(god)} поверг титана: {DefinitionOf(titan).DisplayName}";
            _banner.Announce("Битва титана и бога", text, BannerSeconds);
            _journal.Write(text, JournalTone.Threat);
            _chronicle.Add($"<b>{text}</b>", titan.Position);
        }

        private void OnRoused(Titan titan)
        {
            _banner.Announce("Спящий проснулся", DefinitionOf(titan).DisplayName, BannerSeconds);
            _journal.Write($"Шум мира разбудил: {DefinitionOf(titan).DisplayName}", JournalTone.Threat);
            _chronicle.Add($"<b>Проснулся: {DefinitionOf(titan).DisplayName}</b>", titan.Position);
        }

        private void OnTamed(Titan titan)
        {
            _journal.Write($"Титан приручён и стал стражем: {DefinitionOf(titan).DisplayName}", JournalTone.Good);
            _chronicle.Add($"Приручён титан: {DefinitionOf(titan).DisplayName}", titan.Position);
        }

        private TitanDefinition DefinitionOf(Titan titan) => _config.DefinitionOf(titan.Rule.Id);

        private void OnAwoke(Titan titan)
        {
            var definition = DefinitionOf(titan);
            var avatar = _resolver.Instantiate(_config.AvatarPrefab, _root.Root);
            avatar.name = $"Titan {definition.Id}";
            avatar.Body.Place(_geometry.ToWorld(titan.Position), _geometry.CellSize * definition.Scale);
            avatar.Body.Appear(0f);
            avatar.Tint(definition.Color);
            _avatars[titan] = avatar;

            _banner.Announce("Пробудился титан", $"{definition.DisplayName}\n<size=60%>{definition.Description}</size>", BannerSeconds);
            _journal.Write($"Пробудился титан: {definition.DisplayName}. {definition.Description}", JournalTone.Threat);
            _chronicle.Add($"<b>Пробудился титан: {definition.DisplayName}</b>", titan.Position);
        }

        private void OnMoved(Titan titan)
        {
            if (_avatars.TryGetValue(titan, out var avatar))
                Tween.Position(avatar.Body.transform, _geometry.ToWorld(titan.Position), StepSeconds, Ease.InOutSine);
        }

        private void OnDevoured(Titan titan, God god)
        {
            _journal.Write($"{DefinitionOf(titan).DisplayName} пожрал бога: {_gods.NameOf(god)}", JournalTone.Threat);
            _chronicle.Add($"{DefinitionOf(titan).DisplayName} пожрал бога {_gods.NameOf(god)}", titan.Position);
        }

        private void OnDied(Titan titan)
        {
            Remove(titan);
            _journal.Write($"Титан угас: {DefinitionOf(titan).DisplayName}", JournalTone.Info);
            _chronicle.Add($"Титан угас: {DefinitionOf(titan).DisplayName}", titan.Position);
        }

        private void OnSlain(Titan titan, string by)
        {
            Remove(titan);
            if (by == null) return;

            _banner.Announce("Титан повержен", DefinitionOf(titan).DisplayName, BannerSeconds);
            _journal.Write($"{by} поверг(ли) титана: {DefinitionOf(titan).DisplayName}", JournalTone.Good);
            _chronicle.Add($"<b>{by} поверг(ли) титана: {DefinitionOf(titan).DisplayName}</b>", titan.Position);
        }

        private void Remove(Titan titan)
        {
            if (_avatars.Remove(titan, out var avatar)) avatar.Body.Disappear();
        }
    }
}
