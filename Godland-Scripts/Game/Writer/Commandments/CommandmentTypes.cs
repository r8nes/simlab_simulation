using System.Collections.Generic;
using Godland.Core.World.Grid;

namespace Godland.Game.Writer.Commandments
{
    public enum CommandmentSlot
    {
        When,
        Then,
        Where,
        Amplifier
    }

    public enum CommandmentTrigger
    {
        Death,
        SettlementBorn,
        GodBornOrFallen,
        War,
        EpochChanged,
        TileLost,
        Prayer,
        Hunt
    }

    public enum CommandmentAmplifier
    {
        None,
        Forever,
        Twice,
        Dark,
        Price
    }

    public enum CommandmentRarity
    {
        Common,
        Rare,
        Legendary
    }

    public interface ICommandmentEffect
    {
        bool Apply(CommandmentContext context, GridCoord site, string cause);
    }

    public interface ICommandmentPlace
    {
        IEnumerable<GridCoord> Sites(CommandmentContext context, GridCoord origin);
    }
}
