namespace LouvorHelperCore.Models.Presentation;

/// <summary>
/// Represents a single slide with its content and layout
/// </summary>
internal class Slide
{
    public SlideLayoutType Layout { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public Dictionary<string, object> Properties { get; set; } = new();

    public Slide(SlideLayoutType layout)
    {
        Layout = layout;
    }

    /// <summary>
    /// Creates a title slide with the specified title and artist
    /// </summary>
    public static Slide CreateTitleSlide(string title, string artist)
    {
        return new Slide(SlideLayoutType.Title) { Title = title, Subtitle = $"Por: {artist}" };
    }

    /// <summary>
    /// Creates a lyrics slide with the specified lyrics content
    /// </summary>
    public static Slide CreateLyricsSlide(string lyrics)
    {
        return new Slide(SlideLayoutType.Lyrics) { Content = lyrics };
    }
}
