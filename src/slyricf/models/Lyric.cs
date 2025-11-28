namespace slyricf.models;

class Lyric(List<string>? verses = null)
{
    public List<string> Verses = verses is not null ? verses : [];
}
