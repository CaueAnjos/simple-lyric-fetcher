using slyricf.models;

namespace slyricf;

interface IProvider
{
    Task<Lyric> HtmlToLyric(string html);
}
