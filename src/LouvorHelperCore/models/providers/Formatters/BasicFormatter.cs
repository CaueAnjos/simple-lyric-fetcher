using System.Text.RegularExpressions;

namespace LouvorHelperCore.Models.Providers.Formatters;

public class BasicFormatter : IFormatter
{
    public virtual string Format(string lyrics)
    {
        string cleanedLyrics = lyrics;

        cleanedLyrics = Regex.Replace(cleanedLyrics, @"</p><p>", "\n\n");
        cleanedLyrics = Regex.Replace(cleanedLyrics, @"<p>|</p>", "");
        cleanedLyrics = Regex.Replace(cleanedLyrics, @"<br>|<br/>", "\n");
        cleanedLyrics = Regex.Replace(cleanedLyrics, @"<.*>", "");
        cleanedLyrics = Regex.Replace(cleanedLyrics, @"[A-Z].*\|", "");
        cleanedLyrics = Regex.Replace(cleanedLyrics, @"Parte [0-9]* de [0-9]*", "");
        cleanedLyrics = Regex.Replace(cleanedLyrics, @"[\[\(](.*)[\]\)]", "");
        cleanedLyrics = Regex.Replace(cleanedLyrics, @"[-_]{2,}", "");
        cleanedLyrics = Regex.Replace(cleanedLyrics, @"[\n ]{2,}", "\n\n");

        return cleanedLyrics.Trim();
    }
}
