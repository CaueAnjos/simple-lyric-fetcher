using Spectre.Console;
using Spectre.Console.Cli;

namespace slyricf.commands;

class FetchLyricCommand : Command
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<url>")]
        public string Url { get; set; } = string.Empty;
    }

    public override int Execute(CommandContext context)
    {
        AnsiConsole.MarkupLine("Hello, [cyan]World![/]");
        return 0;
    }
}
