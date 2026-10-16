using System;
using Godland.Core.Powers.Gods;
using Godland.Core.World.Grid;
using Godland.Game.Fate.Epochs;
using Godland.Game.Powers.Gods;
using Godland.Game.Presentation.History;
using Godland.Game.Presentation.Journal;
using VContainer;
using VContainer.Unity;

namespace Godland.Game.Powers.Pantheons
{
    public sealed class PantheonsPresenter : IStartable, IDisposable
    {
        private const float BannerSeconds = 3f;

        private readonly PantheonKeeper _keeper;
        private readonly GodsConfig _gods;
        private readonly EventJournal _journal;
        private readonly Chronicle _chronicle;
        private readonly EpochView _banner;
        private readonly WorldSchemes _world;

        public PantheonsPresenter(PantheonKeeper keeper, GodsConfig gods, EventJournal journal, Chronicle chronicle, EpochView banner, WorldSchemes world)
        {
            _world = world;
            _keeper = keeper;
            _gods = gods;
            _journal = journal;
            _chronicle = chronicle;
            _banner = banner;
        }

        public void Start()
        {
            _keeper.Arrived += OnArrived;
            _keeper.GodBorn += OnGodBorn;
            _keeper.Acted += OnActed;
            _keeper.BondChanged += OnBondChanged;
            _keeper.Battled += OnBattled;
            _keeper.Succeeded += OnSucceeded;
            _keeper.Faded += OnFaded;
            _keeper.Absorbed += OnAbsorbed;
            _keeper.Schemed += OnSchemed;
            _keeper.SchemeFulfilled += OnFulfilled;
            _world.Schemed += OnWorldSchemed;
            _world.Fulfilled += OnWorldFulfilled;
        }

        public void Dispose()
        {
            _keeper.Arrived -= OnArrived;
            _keeper.GodBorn -= OnGodBorn;
            _keeper.Acted -= OnActed;
            _keeper.BondChanged -= OnBondChanged;
            _keeper.Battled -= OnBattled;
            _keeper.Succeeded -= OnSucceeded;
            _keeper.Faded -= OnFaded;
            _keeper.Absorbed -= OnAbsorbed;
            _keeper.Schemed -= OnSchemed;
            _keeper.SchemeFulfilled -= OnFulfilled;
            _world.Schemed -= OnWorldSchemed;
            _world.Fulfilled -= OnWorldFulfilled;
        }

        private void OnArrived(PantheonHouse house, string cause)
        {
            _banner.Announce("Пришёл пантеон", house.Definition.DisplayName, BannerSeconds);
            Write($"Пришёл пантеон «{house.Definition.DisplayName}» во главе с {_gods.NameOf(house.Head)}: {cause}", JournalTone.Good, house.Head.Seat);
        }

        private void OnGodBorn(PantheonHouse house, God god) =>
            Write($"В пантеоне «{house.Definition.DisplayName}» родился бог: {_gods.NameOf(god)}", JournalTone.Good, god.Seat);

        private void OnActed(PantheonHouse house, PantheonDeed deed, GridCoord site, string reason) =>
            _journal.Write($"{house.Definition.DisplayName}: {deed.Name} ({reason})");

        private void OnBondChanged(PantheonHouse first, PantheonHouse second, PantheonBond bond)
        {
            var text = bond switch
            {
                PantheonBond.War => $"Священная война: «{first.Definition.DisplayName}» против «{second.Definition.DisplayName}»",
                PantheonBond.Alliance => $"Союз богов: «{first.Definition.DisplayName}» и «{second.Definition.DisplayName}»",
                _ => $"Пантеоны «{first.Definition.DisplayName}» и «{second.Definition.DisplayName}» снова настороже"
            };
            if (bond == PantheonBond.War) _banner.Announce("Священная война", $"{first.Definition.DisplayName} — {second.Definition.DisplayName}", BannerSeconds);
            Write(text, bond == PantheonBond.War ? JournalTone.Threat : JournalTone.Info, null);
        }

        private void OnBattled(PantheonHouse winner, PantheonHouse loser, GridCoord target) =>
            _journal.Write($"Священная война: верующие «{winner.Definition.DisplayName}» бьют «{loser.Definition.DisplayName}»", JournalTone.Threat);

        private void OnSucceeded(PantheonHouse house, God heir) =>
            Write($"Глава пантеона «{house.Definition.DisplayName}» пал, престол занял {_gods.NameOf(heir)}", JournalTone.Threat, heir.Seat);

        private void OnFaded(PantheonHouse house, string cause) =>
            Write($"Пантеон «{house.Definition.DisplayName}» угас: {cause}", JournalTone.Threat, null);

        private void OnAbsorbed(PantheonHouse strong, PantheonHouse weak) =>
            Write($"Пантеон «{weak.Definition.DisplayName}» растворился в «{strong.Definition.DisplayName}»", JournalTone.Info, strong.Head?.Seat);

        private void OnSchemed(PantheonHouse house, God god, GodAction action, GridCoord site) =>
            Write(string.IsNullOrEmpty(action.Text) ? $"{_gods.NameOf(god)}: {action.Name}" : $"{_gods.NameOf(god)}: {action.Text}", JournalTone.Info, site);

        private void OnFulfilled(PantheonHouse house, God god, GodScheme scheme) =>
            _banner.Announce(house.Definition.DisplayName, $"Замысел исполнен: {_gods.NameOf(god)}", BannerSeconds);

        private void OnWorldSchemed(string actor, GodAction action, GridCoord site) =>
            Write(string.IsNullOrEmpty(action.Text) ? $"{actor}: {action.Name}" : $"{actor} {action.Text}", JournalTone.Info, site);

        private void OnWorldFulfilled(string actor, GodScheme scheme) => _banner.Announce("Замысел исполнен", actor, BannerSeconds);

        private void Write(string text, JournalTone tone, GridCoord? place)
        {
            _journal.Write(text, tone);
            _chronicle.Add(text, place);
        }
    }

    public sealed class PantheonsInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<PantheonsPresenter>();
        }
    }
}
