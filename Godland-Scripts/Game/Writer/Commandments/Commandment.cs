namespace Godland.Game.Writer.Commandments
{
    public sealed class Commandment
    {
        public Commandment(CommandmentWord when, CommandmentWord then, CommandmentWord where, CommandmentWord amplifier)
        {
            When = when;
            Then = then;
            Where = where;
            Amplifier = amplifier;
        }

        public CommandmentWord When { get; }
        public CommandmentWord Then { get; }
        public CommandmentWord Where { get; }
        public CommandmentWord Amplifier { get; }

        public CommandmentAmplifier Boost => Amplifier == null ? CommandmentAmplifier.None : Amplifier.Amplifier;

        public string Text => $"Когда {When.Text}, {Then.Text} {Where.Text}" + (Amplifier == null ? string.Empty : $", {Amplifier.Text}");
    }
}
