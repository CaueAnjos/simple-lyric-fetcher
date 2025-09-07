namespace LouvorHelperCore.Models;

public class Music
{
    public Music(string title, string artist, string lyrics)
    {
        Title = title;
        Artist = artist;
        Lyrics = lyrics;
    }

    public string Title { get; set; }
    public string Artist { get; set; }
    public string Lyrics { get; set; }
}
