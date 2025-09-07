using LouvorHelperCore.Models.Providers.Formatters;

namespace LouvorHelperCore.Models.Providers;

public abstract class Provider
{
    protected Provider(string url, string label, IFormatter formatter)
    {
        Url = url;
        Label = label;
        Formatter = formatter;
    }

    public string Url { get; protected set; }
    public string Label { get; }
    public IFormatter Formatter { get; protected set; }

    protected abstract string BuildUrl(string title, string artist);

    public virtual async Task<string?> GetLyricsAsync(string title, string artist)
    {
        HttpClient client = new();
        try
        {
            string url = BuildUrl(title, artist);
            string rawLyrics = await client.GetStringAsync(url);
            return Formatter.Format(rawLyrics);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }
}
