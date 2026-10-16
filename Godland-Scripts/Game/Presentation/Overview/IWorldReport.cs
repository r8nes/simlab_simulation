using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Grid;
using Godland.Game.Writer.Commands;

namespace Godland.Game.Presentation.Overview
{
    public sealed class WorldReportLine
    {
        public WorldReportLine(string title, string text, GridCoord? place = null, IEnumerable<EntityCommand> commands = null)
        {
            Commands = commands?.Where(command => command != null).ToList() ?? new List<EntityCommand>();
            Title = title;
            Text = text;
            Place = place;
        }

        public string Title { get; }
        public string Text { get; }
        public GridCoord? Place { get; }
        public IReadOnlyList<EntityCommand> Commands { get; }
    }

    public interface IWorldReport
    {
        string Title { get; }
        IEnumerable<WorldReportLine> Lines();
    }
}
