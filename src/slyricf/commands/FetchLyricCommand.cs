using System.Text;
using slyricf.models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace slyricf.commands;

class FetchLyricCommand : Command<FetchLyricCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<url>")]
        public string Url { get; set; } = string.Empty;

        [CommandOption("-o|--output")]
        public string? OutputPath { get; set; }
    }

    public override ValidationResult Validate(CommandContext context, Settings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Url))
        {
            return ValidationResult.Error("URL cannot be empty");
        }

        if (
            !Uri.TryCreate(settings.Url, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        )
        {
            return ValidationResult.Error("Invalid URL format. Must be a valid HTTP or HTTPS URL");
        }

        return FetchLyricCommand.ValidateOutputPath(settings.OutputPath);
    }

    private static ValidationResult ValidateOutputPath(string? outputPath)
    {
        if (outputPath is null)
            return ValidationResult.Success();

        if (string.IsNullOrWhiteSpace(outputPath))
            return ValidationResult.Error("Output path cannot be null or empty.");

        try
        {
            string fullPath = Path.GetFullPath(outputPath);

            if (Path.GetInvalidPathChars().Any(invalidChar => fullPath.Contains(invalidChar)))
                return ValidationResult.Error("Output path contains invalid characters.");

            string directoryPath = Path.GetDirectoryName(fullPath)!;
            if (!Directory.Exists(directoryPath))
            {
                try
                {
                    Directory.CreateDirectory(directoryPath);
                }
                catch (Exception)
                {
                    return ValidationResult.Error("Cannot create the output directory.");
                }
            }

            try
            {
                using (
                    FileStream fs = File.Create(
                        Path.Combine(directoryPath, Path.GetRandomFileName()),
                        1,
                        FileOptions.DeleteOnClose
                    )
                ) { }
            }
            catch
            {
                return ValidationResult.Error("Insufficient permissions for the output path.");
            }

            return ValidationResult.Success();
        }
        catch (Exception ex)
        {
            return ValidationResult.Error($"Invalid output path: {ex.Message}");
        }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        return ExecuteAsync(context, settings).GetAwaiter().GetResult();
    }

    private async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        try
        {
            AnsiConsole.MarkupLine($"[blue]Fetching lyrics from:[/] [yellow]{settings.Url}[/]");
            AnsiConsole.WriteLine();

            var content = await AnsiConsole
                .Status()
                .StartAsync(
                    "Downloading...",
                    async ctx =>
                    {
                        ctx.Spinner(Spinner.Known.Dots);
                        ctx.SpinnerStyle(Style.Parse("green"));
                        return await GetContentFromUrl(settings.Url);
                    }
                );

            AnsiConsole.MarkupLine("[green]✓[/] Lyrics fetched successfully!");
            AnsiConsole.WriteLine();

            var lyric = await GetLyricFromContent(content, settings.Url);

            if (settings.OutputPath is not null)
            {
                await SaveLyric(lyric, settings.OutputPath);
            }
            else
            {
                PrintLyric(lyric);
            }

            return 0;
        }
        catch (HttpRequestException ex)
        {
            AnsiConsole.MarkupLine($"[red]HTTP Error:[/] {ex.Message}");
            return 1;
        }
        catch (TaskCanceledException)
        {
            AnsiConsole.MarkupLine("[red]Request timed out[/]");
            return 1;
        }
    }

    private async Task SaveLyric(Lyric lyric, string path)
    {
        string finalPath = Path.ChangeExtension(path, "md");
        string lyricContent = string.Join("\n\n---\n\n", lyric.Verses);
        await File.WriteAllTextAsync(finalPath, lyricContent, Encoding.UTF8);
    }

    private void PrintLyric(Lyric lyric)
    {
        var escapedContent = lyric.ToString();

        var panel = new Panel(escapedContent)
        {
            Header = new PanelHeader("Lyrics", Justify.Center),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Green),
        };

        AnsiConsole.Write(panel);
    }

    private async Task<string> GetContentFromUrl(string url)
    {
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(30);
        httpClient.DefaultRequestHeaders.Add("User-Agent", "SlyricF/1.0");

        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    private async Task<Lyric> GetLyricFromContent(string content, string url)
    {
        var provider = new LetraMusProvider();
        if (!provider.CheckUrl(url))
            throw new NullReferenceException(
                "Internal error while scraping. Please check your link!"
            );

        return await provider.HtmlToLyric(content);
    }
}
