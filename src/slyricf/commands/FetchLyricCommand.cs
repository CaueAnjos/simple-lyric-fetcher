using Spectre.Console;
using Spectre.Console.Cli;

namespace slyricf.commands;

class FetchLyricCommand : Command
{
    public override int Execute(CommandContext context)
    {
        AnsiConsole.MarkupLine("Hello, [cyan]World![/]");
        return 0;
    }
}
