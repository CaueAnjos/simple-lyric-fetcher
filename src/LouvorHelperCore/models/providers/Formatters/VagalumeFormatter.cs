using System.Text.RegularExpressions;

namespace LouvorHelperCore.Models.Providers.Formatters;

public class VagalumeFormatter : BasicFormatter
{
    public override string Format(string lyrics)
    {
        var match = Regex.Match(
            lyrics,
            @"<div id=lyrics[^>]*>(.*?)</div>",
            RegexOptions.Singleline
        );

        if (!match.Success)
            return string.Empty;

        string rawLyrics = match.Groups[1].Value;
        return base.Format(rawLyrics);
    }
}
