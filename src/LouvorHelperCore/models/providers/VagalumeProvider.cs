using System.Text;
using LouvorHelperCore.Models.Providers.Formatters;
using LouvorHelperCore.Utils;

namespace LouvorHelperCore.Models.Providers;

public class VagalumeProvider : Provider
{
    public VagalumeProvider()
        : base(
            label: "Vagalume",
            url: "https://www.vagalume.com.br/",
            formatter: new VagalumeFormatter()
        ) { }

    protected override string BuildUrl(string title, string artist)
    {
        StringBuilder url = new(Url);
        url.Append(PrepareString(artist));
        url.Append('/');
        url.Append(PrepareString(title));
        url.Append(".html");
        return url.ToString();
    }

    private string PrepareString(string str)
    {
        return StringUtils.RemoveDiacritics(str.ToLower().Replace(' ', '-'));
    }
}
