using System.Text.RegularExpressions;
using LouvorHelperCore.Utils;

namespace LouvorHelperCore.Models.Providers.Formatters;

public class LetrasMusFormatter : BasicFormatter
{
    public override string Format(string lyrics)
    {
        var match = Regex.Match(
            lyrics,
            @"<div class=""lyric-original"">(.*?)</div>",
            RegexOptions.Singleline
        );

        if (!match.Success)
            return string.Empty;

        string rawLyrics = match.Groups[1].Value;

        rawLyrics = base.Format(rawLyrics);
        return rawLyrics;
    }
}
