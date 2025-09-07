using System.CommandLine;
using System.Text;
using LouvorHelperCore.Models;
using LouvorHelperCore.Utils;
using Spectre.Console;

namespace LouvorHelper.Commands;

internal class MedleyCommand : Command
{
    public MedleyCommand()
        : base("medley", "Cria um medley das musicas especificadas")
    {
        Option<string?> authorOption = new(
            ["--author", "--autor", "-a"],
            "define o autor do medley"
        );
        this.SetHandler(CommandAction, authorOption);
    }

    bool NotifyErrorAndConfirm(string message)
    {
        Notify.Error(message);
        return Notify.YesNoBox("Deseja continuar com outro arquivo?", true);
    }

    private async Task CommandAction(string? author)
    {
        FileManager fileManager = new();
        author ??= "LouvorHelper";

        var lyricsBuilder = new StringBuilder();
        var titleBuilder = new StringBuilder("(Medley) ");

        var musics = fileManager.GetMusicFiles().ToList();

        var musicsForMedley = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title("Select [green]musics[/] to include in the [cyan]medley[/]")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more musics)[/]")
                .InstructionsText(
                    "[grey](Press [blue]<space>[/] to toggle a music, "
                        + "[green]<enter>[/] to accept)[/]"
                )
                .AddChoices(musics)
        );

        foreach (string fileName in musicsForMedley)
        {
            Music? musicFromFile = await fileManager.LoadAsync(fileName);
            if (musicFromFile is null)
            {
                AnsiConsole.MarkupLine($"[red]Não foi possível carregar o arquivo {fileName}[/]");
                var confirmation = AnsiConsole.Prompt(
                    new TextPrompt<bool>("Want to continue with out this file?")
                        .AddChoice(true)
                        .AddChoice(false)
                        .DefaultValue(true)
                        .WithConverter(choice => choice ? "y" : "n")
                );

                if (!confirmation)
                    break;
            }
            else
            {
                lyricsBuilder.AppendLine(musicFromFile.Lyrics).AppendLine();
                titleBuilder.Append(musicFromFile.Title).Append('+');
            }
        }

        if (musicsForMedley.Count < 2)
        {
            AnsiConsole.MarkupLine("[red]No valid file was processed to create the medley.[/]");
            return;
        }

        string finalTitle = titleBuilder.ToString().TrimEnd('+');
        string finalLyrics = lyricsBuilder.ToString().Trim();

        var panel = new Panel($"[gray]{finalLyrics}[/]\n");
        panel.Border(BoxBorder.Rounded);
        panel.Header("lyrics");
        AnsiConsole.Write(panel);

        var confimationMedley = AnsiConsole.Prompt(
            new TextPrompt<bool>("Want to continue with this [cyan]medley[/]?")
                .AddChoice(true)
                .AddChoice(false)
                .DefaultValue(true)
                .WithConverter(choice => choice ? "y" : "n")
        );

        if (confimationMedley)
        {
            Music medley = new(finalTitle, author, finalLyrics);
            await fileManager.SaveAsync(medley);
            AnsiConsole.MarkupLine($"[green]File saved at: {fileManager.DownloadPath}[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Medley not saved.[/]");
        }
    }
}
