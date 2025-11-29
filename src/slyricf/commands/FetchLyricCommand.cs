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

        return ValidationResult.Success();
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
            var escapedContent = lyric?.ToString();
            if (escapedContent is null)
            {
                throw new NullReferenceException("Internal error on scraping");
            }

            var panel = new Panel(escapedContent)
            {
                Header = new PanelHeader("Lyrics", Justify.Center),
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.Green),
            };

            AnsiConsole.Write(panel);

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

    private async Task<string> GetContentFromUrl(string url)
    {
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(30);
        httpClient.DefaultRequestHeaders.Add("User-Agent", "SlyricF/1.0");

        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    private async Task<Lyric?> GetLyricFromContent(string content, string url)
    {
        var provider = new LetraMusProvider();
        if (!provider.CheckUrl(url))
            return null;

        return await provider.HtmlToLyric(content);
    }
}
