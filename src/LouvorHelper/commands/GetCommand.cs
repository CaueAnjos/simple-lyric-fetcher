using System.CommandLine;
using LouvorHelperCore.Models;
using LouvorHelperCore.Models.Providers;
using LouvorHelperCore.Utils;
using Spectre.Console;
using Spectre.Console.Extensions;

namespace LouvorHelper.Commands;

internal class GetCommand : Command
{
    public GetCommand()
        : base("get", "Faz o download dos arquivos de músicas de Louvor")
    {
        var titleOption = new Option<string>(["--title", "--titulo", "-t"], "O título da música")
        {
            IsRequired = true,
        };
        var authorOption = new Option<string>(["--author", "--autor", "-a"], "O nome do autor")
        {
            IsRequired = true,
        };
        var autoCompileOption = new Option<bool>(
            ["--auto-compile", "--compilar", "-c"],
            getDefaultValue: () => true,
            "Compila automaticamente as apresentações"
        );
        var capslockOption = new Option<bool>(
            ["--upper", "--fixa", "-U"],
            getDefaultValue: () => false,
            "Deixa as letras maiúsculas"
        );

        AddOption(titleOption);
        AddOption(authorOption);
        AddOption(autoCompileOption);
        AddOption(capslockOption);

        this.SetHandler(
            CommandAction,
            titleOption,
            authorOption,
            autoCompileOption,
            capslockOption
        );
    }

    private async Task CommandAction(string title, string author, bool autoCompile, bool capslock)
    {
        ProviderContainer providerContainer = new(
            [new VagalumeProvider(), new CifraClubProvider(), new LetrasMusProvider()]
        );
        await providerContainer.GetLyricsAsync(title, author).Spinner();

        string? lyrics = SelectProvider(providerContainer);

        if (lyrics is not null)
        {
            if (capslock)
            {
                lyrics = lyrics.ToUpper();
            }

            var panel = new Panel($"[gray]{lyrics}[/]\n");
            panel.Border(BoxBorder.Rounded);
            panel.Header("lyrics");
            AnsiConsole.Write(panel);

            var confimation = AnsiConsole.Prompt(
                new TextPrompt<bool>("Want to continue with this [cyan]music[/]?")
                    .AddChoice(true)
                    .AddChoice(false)
                    .DefaultValue(true)
                    .WithConverter(choice => choice ? "y" : "n")
            );

            if (!confimation)
            {
                AnsiConsole.MarkupLine("[red]Music not saved.[/]");
                return;
            }

            Music music = new(title, author, lyrics);
            FileManager fileManager = new();
            await fileManager.SaveAsync(music);

            AnsiConsole.MarkupLine($"[gray]Saved at: {fileManager.DownloadPath}[/]");

            if (autoCompile)
            {
                PresentationCompiler compiler = new();
                compiler.CompileMusic(music);
            }

            AnsiConsole.MarkupLine("[green]Done![/]");
        }
    }

    private string? SelectProvider(ProviderContainer container)
    {
        if (container.GoodProvidersResponse == 0)
        {
            AnsiConsole.MarkupLine($"[red]Lyrics not found[/]");
            return null;
        }

        if (container.GoodProvidersResponse == 1)
        {
            AnsiConsole.MarkupLine("[cyan]Using default provider[/]");
            return container.GetDefaultLyrics();
        }

        string choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Which [cyan]provider[/] do you want to use?")
                .PageSize(3)
                .AddChoices(
                    container
                        .Lyrics.Where(k => !string.IsNullOrEmpty(k.Value))
                        .Select(k => k.Key)
                        .OrderBy(k => k)
                )
        );

        AnsiConsole.MarkupLine($"Using [cyan]{choice}[/]");

        return container.Lyrics[choice];
    }
}
