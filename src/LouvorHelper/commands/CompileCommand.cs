using System.CommandLine;
using LouvorHelperCore.Models;
using Spectre.Console;

namespace LouvorHelper.Commands;

internal class CompileCommand : Command
{
    public CompileCommand()
        : base("compile", "Compila os arquivos de músicas de Louvor")
    {
        Option<string?> templateOption = new(
            ["--template", "-m"],
            "O caminho para o modelo que será aplicado as apresentações"
        );
        AddOption(templateOption);
        this.SetHandler(CommandAction, templateOption);
    }

    private async Task CommandAction(string? templatePath)
    {
        PresentationCompiler compiler = new();
        await AnsiConsole
            .Status()
            .StartAsync(
                "Compiling...",
                async ctx =>
                {
                    if (templatePath is not null)
                        compiler.TemplatePath = templatePath;

                    await foreach (Music music in compiler.FileManager.LoadAsync())
                    {
                        AnsiConsole.MarkupLine($"[gray]At {music.Title}[/]");
                        compiler.CompileMusic(music);
                        await Task.Delay(500);
                        AnsiConsole.MarkupLine($"[green]Compiled[/] [cyan]{music.Title}[/]");
                    }
                }
            );
        AnsiConsole.MarkupLine($"[gray]compiled at: {compiler.FileManager.CompileOutputPath}[/]");
        AnsiConsole.MarkupLine("[green]Done![/]");
    }
}
