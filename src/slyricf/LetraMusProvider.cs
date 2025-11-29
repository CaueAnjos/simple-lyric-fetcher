using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using slyricf.models;

namespace slyricf;

class LetraMusProvider : IProvider
{
    public bool CheckUrl(string url)
    {
        return url.Contains("letras.mus");
    }

    public async Task<Lyric> HtmlToLyric(string html)
    {
        var context = BrowsingContext.New(Configuration.Default);
        var parser = context.GetService<IHtmlParser>();
        if (parser is null)
            throw new NullReferenceException("parser was null for some reason");

        var document = await parser.ParseDocumentAsync(html);

        var lyricDiv = document.QuerySelector(".lyric-original");

        if (lyricDiv is null)
        {
            return new Lyric();
        }

        var verses = new List<string>();

        var paragraphs = lyricDiv.QuerySelectorAll("p");

        foreach (var paragraph in paragraphs)
        {
            var annotations = paragraph.QuerySelectorAll(".annotationWrapper, .lyricAnnotation");
            foreach (var annotation in annotations)
            {
                var textContent = annotation.TextContent;
                annotation.OuterHtml = textContent;
            }

            var verseText = GetTextWithLineBreaks(paragraph);

            verseText = Regex.Replace(verseText.Trim(), @"\n{2,}", "\n");

            if (!string.IsNullOrWhiteSpace(verseText))
            {
                verses.Add(verseText);
            }
        }

        return new Lyric(verses);
    }

    private static string GetTextWithLineBreaks(IElement element)
    {
        var result = "";

        foreach (var child in element.ChildNodes)
        {
            if (child.NodeType == NodeType.Text)
            {
                result += child.TextContent;
            }
            else if (child.NodeName.ToLower() == "br")
            {
                result += "\n";
            }
            else if (child is IElement childElement)
            {
                result += GetTextWithLineBreaks(childElement);
            }
        }

        return result;
    }
}
