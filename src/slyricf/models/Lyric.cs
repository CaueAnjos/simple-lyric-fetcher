namespace slyricf.models;

class Lyric
{
    public List<string> Verses;

    public Lyric(List<string> verses)
    {
        Verses = verses;
    }

    public Lyric()
    {
        Verses = [];
    }
}
