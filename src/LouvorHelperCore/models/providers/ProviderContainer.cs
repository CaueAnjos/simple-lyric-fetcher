namespace LouvorHelperCore.Models.Providers;

public class ProviderContainer
{
    public List<Provider> Providers { get; private set; }
    public Dictionary<string, string?> Lyrics { get; private set; }

    public ProviderContainer(List<Provider> providers)
    {
        Providers = providers;
        Lyrics = new();
    }

    public async Task GetLyricsAsync(string title, string artist)
    {
        var tasks = Providers
            .Select(async provider =>
            {
                var lyrics = await provider.GetLyricsAsync(title, artist);
                Lyrics[provider.Label] = lyrics;
            })
            .ToList();

        await Task.WhenAll(tasks);
    }

    public string? GetDefaultLyrics()
    {
        return Lyrics.FirstOrDefault(t => !string.IsNullOrEmpty(t.Value)).Value;
    }

    public int GoodProvidersResponse => Lyrics.Count(t => !string.IsNullOrEmpty(t.Value));
}
