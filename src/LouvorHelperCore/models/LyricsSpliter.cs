namespace LouvorHelperCore.Models;

/// <summary>
/// Utility class for splitting lyrics into manageable sections for slides
/// </summary>
internal static class LyricsSplitter
{
    /// <summary>
    /// Splits lyrics into manageable sections for slides
    /// </summary>
    /// <param name="lyrics">The full lyrics text</param>
    /// <param name="maxLinesPerSlide">Maximum number of lines per slide (default: 8)</param>
    /// <returns>List of lyric sections</returns>
    public static List<string> SplitLyrics(string lyrics, int maxLinesPerSlide = 5)
    {
        var sections = new List<string>();
        if (string.IsNullOrWhiteSpace(lyrics))
            return sections;

        var lines = lyrics.Split(['\n', '\r'], StringSplitOptions.TrimEntries);
        var currentSection = new List<string>();

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                // Empty line indicates section break
                if (currentSection.Count > 0)
                {
                    sections.Add(string.Join("\n", currentSection));
                    currentSection.Clear();
                }
            }
            else
            {
                currentSection.Add(line);

                // If section gets too long, split it
                if (currentSection.Count >= maxLinesPerSlide)
                {
                    sections.Add(string.Join("\n", currentSection));
                    currentSection.Clear();
                }
            }
        }

        // Add remaining lines
        if (currentSection.Count > 0)
        {
            sections.Add(string.Join("\n", currentSection));
        }

        return sections.Count > 0 ? sections : new List<string> { lyrics };
    }

    /// <summary>
    /// Splits lyrics by verses and choruses, keeping them separate
    /// </summary>
    /// <param name="lyrics">The full lyrics text</param>
    /// <returns>List of lyric sections organized by verses/choruses</returns>
    public static List<string> SplitByVersesAndChorus(string lyrics)
    {
        var sections = new List<string>();
        if (string.IsNullOrWhiteSpace(lyrics))
            return sections;

        // Split by double line breaks (typical verse/chorus separators)
        var paragraphs = lyrics.Split(
            ["\n\n", "\r\n\r\n", "\r\r"],
            StringSplitOptions.RemoveEmptyEntries
        );

        foreach (var paragraph in paragraphs)
        {
            var trimmed = paragraph.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                sections.Add(trimmed);
            }
        }

        return sections.Count > 0 ? sections : SplitLyrics(lyrics);
    }

    /// <summary>
    /// Counts the number of lines in a text block
    /// </summary>
    /// <param name="text">The text to count lines in</param>
    /// <returns>Number of lines</returns>
    public static int CountLines(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        return text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    /// <summary>
    /// Checks if a text block is too long for a single slide
    /// </summary>
    /// <param name="text">The text to check</param>
    /// <param name="maxLines">Maximum lines per slide (default: 8)</param>
    /// <returns>True if the text is too long</returns>
    public static bool IsTooLongForSlide(string text, int maxLines = 8)
    {
        return CountLines(text) > maxLines;
    }
}
