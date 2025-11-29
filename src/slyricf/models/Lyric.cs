using System.Text;

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

    public override string ToString()
    {
        var builder = new StringBuilder();
        foreach (var verse in Verses)
        {
            builder.AppendLine(verse);
            builder.AppendLine();
        }
        return builder.ToString().Trim();
    }
}
