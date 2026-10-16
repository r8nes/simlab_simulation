using System.Linq;
using Godland.Core.Powers.Gods;
using Godland.Core.Life.People;
using Godland.Core.World.Tiles;
using Godland.Game.Powers.Gods;
using Godland.Game.Life.People;
using Godland.Game.World.Tiles;

namespace Godland.Game.Presentation.Overview
{
    public sealed class FaithNames
    {
        private readonly Pantheon _pantheon;
        private readonly GodsConfig _gods;
        private readonly SettlementConfig _settlements;
        private readonly WorldTiles _tiles;
        private readonly TileCatalog _catalog;

        public FaithNames(Pantheon pantheon, GodsConfig gods, SettlementConfig settlements, WorldTiles tiles, TileCatalog catalog)
        {
            _pantheon = pantheon;
            _gods = gods;
            _settlements = settlements;
            _tiles = tiles;
            _catalog = catalog;
        }

        public string Of(Religion religion)
        {
            var heresy = religion.IsHeresy ? " (ересь)" : string.Empty;
            var god = _pantheon.Gods.FirstOrDefault(candidate => candidate.Religion == religion);
            if (god != null) return $"вера в бога {_gods.NameOf(god)}{heresy}";
            if (religion.IdolId != null) return $"вера в идола {_settlements.IdolById(religion.IdolId).FullName}{heresy}";
            return religion.Shrine is { } shrine && _tiles.TryGet(shrine, out var tile) ? $"вера в святыню «{_catalog.Get(tile).DisplayName}»{heresy}" : $"безымянная вера{heresy}";
        }

        public string DogmaOf(Religion religion) => _settlements.DogmaById(religion.Dogma.Id)?.DisplayName ?? religion.Dogma.Id;
    }
}
