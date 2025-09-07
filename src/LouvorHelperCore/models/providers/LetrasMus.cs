using System.Text;
using LouvorHelperCore.Models.Providers.Formatters;
using LouvorHelperCore.Utils;

namespace LouvorHelperCore.Models.Providers;

public class LetrasMusProvider : Provider
{
    public LetrasMusProvider()
        : base(
            label: "LetrasMus",
            url: "https://www.letras.mus.br/",
            formatter: new LetrasMusFormatter()
        ) { }

    protected override string BuildUrl(string title, string artist)
    {
        StringBuilder url = new(Url);
        url.Append(PrepareString(artist));
        url.Append('/');
        url.Append(PrepareString(title));
        url.Append('/');
        return url.ToString();
    }

    private string PrepareString(string str)
    {
        return StringUtils.RemoveDiacritics(str.ToLower().Replace(' ', '-'));
    }
}
