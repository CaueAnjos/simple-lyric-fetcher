using DocumentFormat.OpenXml.Packaging;
using D = DocumentFormat.OpenXml.Drawing;
using P = DocumentFormat.OpenXml.Presentation;

namespace LouvorHelperCore.Models.Presentation;

/// <summary>
/// Factory for creating OpenXML slide objects
/// </summary>
internal static class SlideFactory
{
    /// <summary>
    /// Creates an OpenXML slide from a Slide object
    /// </summary>
    public static P.Slide CreateSlide(Slide slide)
    {
        return slide.Layout switch
        {
            SlideLayoutType.Title => CreateTitleSlide(slide.Title, slide.Subtitle),
            SlideLayoutType.Lyrics => CreateLyricSlide(slide.Content),
            _ => throw new ArgumentException($"Unsupported layout type: {slide.Layout}"),
        };
    }

    /// <summary>
    /// Creates an OpenXML slide with proper layout reference for theme inheritance
    /// </summary>
    public static P.Slide CreateSlideWithLayout(Slide slide, SlideLayoutPart layoutPart)
    {
        var openXmlSlide = slide.Layout switch
        {
            SlideLayoutType.Title => CreateTitleSlideWithTheme(slide.Title, slide.Subtitle),
            SlideLayoutType.Lyrics => CreateLyricSlideWithTheme(slide.Content),
            _ => throw new ArgumentException($"Unsupported layout type: {slide.Layout}"),
        };

        string layoutId = "rId1";
        if (layoutPart.SlideMasterPart is not null)
        {
            // Add layout relationship reference
            layoutId = layoutPart.GetIdOfPart(layoutPart.SlideMasterPart);
        }

        return openXmlSlide;
    }

    private static P.Slide CreateTitleSlide(string title, string subtitle)
    {
        return new P.Slide(
            new P.CommonSlideData(
                new P.ShapeTree(
                    new P.NonVisualGroupShapeProperties(
                        new P.NonVisualDrawingProperties() { Id = 1, Name = "" },
                        new P.NonVisualGroupShapeDrawingProperties(),
                        new P.ApplicationNonVisualDrawingProperties()
                    ),
                    new P.GroupShapeProperties(new D.TransformGroup()),
                    // Title shape
                    CreateTitleShape(title),
                    // Subtitle shape
                    CreateSubtitleShape(subtitle)
                )
            ),
            new P.ColorMapOverride(new D.MasterColorMapping())
        );
    }

    private static P.Slide CreateTitleSlideWithTheme(string title, string subtitle)
    {
        return new P.Slide(
            new P.CommonSlideData(
                new P.ShapeTree(
                    new P.NonVisualGroupShapeProperties(
                        new P.NonVisualDrawingProperties() { Id = 1, Name = "" },
                        new P.NonVisualGroupShapeDrawingProperties(),
                        new P.ApplicationNonVisualDrawingProperties()
                    ),
                    new P.GroupShapeProperties(new D.TransformGroup()),
                    // Title shape with theme support
                    CreateTitleShapeWithTheme(title),
                    // Subtitle shape with theme support
                    CreateSubtitleShapeWithTheme(subtitle)
                )
            ),
            new P.ColorMapOverride(new D.MasterColorMapping())
        );
    }

    private static P.Slide CreateLyricSlide(string lyrics)
    {
        return new P.Slide(
            new P.CommonSlideData(
                new P.ShapeTree(
                    new P.NonVisualGroupShapeProperties(
                        new P.NonVisualDrawingProperties() { Id = 1, Name = "" },
                        new P.NonVisualGroupShapeDrawingProperties(),
                        new P.ApplicationNonVisualDrawingProperties()
                    ),
                    new P.GroupShapeProperties(new D.TransformGroup()),
                    // Lyrics shape
                    CreateLyricsShape(lyrics)
                )
            ),
            new P.ColorMapOverride(new D.MasterColorMapping())
        );
    }

    private static P.Slide CreateLyricSlideWithTheme(string lyrics)
    {
        return new P.Slide(
            new P.CommonSlideData(
                new P.ShapeTree(
                    new P.NonVisualGroupShapeProperties(
                        new P.NonVisualDrawingProperties() { Id = 1, Name = "" },
                        new P.NonVisualGroupShapeDrawingProperties(),
                        new P.ApplicationNonVisualDrawingProperties()
                    ),
                    new P.GroupShapeProperties(new D.TransformGroup()),
                    // Lyrics shape with theme support
                    CreateLyricsShapeWithTheme(lyrics)
                )
            ),
            new P.ColorMapOverride(new D.MasterColorMapping())
        );
    }

    private static P.Shape CreateTitleShape(string title)
    {
        return new P.Shape(
            new P.NonVisualShapeProperties(
                new P.NonVisualDrawingProperties() { Id = 2, Name = "Title" },
                new P.NonVisualShapeDrawingProperties(new D.ShapeLocks() { NoGrouping = true }),
                new P.ApplicationNonVisualDrawingProperties(
                    new P.PlaceholderShape() { Type = P.PlaceholderValues.Title }
                )
            ),
            new P.ShapeProperties(
                new D.Transform2D(
                    new D.Offset() { X = 914400L, Y = 1828800L },
                    new D.Extents() { Cx = 10297200L, Cy = 1368296L }
                )
            ),
            new P.TextBody(
                new D.BodyProperties() { Anchor = D.TextAnchoringTypeValues.Center },
                new D.ListStyle(),
                new D.Paragraph(
                    new D.ParagraphProperties() { Alignment = D.TextAlignmentTypeValues.Center },
                    new D.Run(
                        new D.RunProperties() { FontSize = 4400, Bold = true },
                        new D.Text(title)
                    )
                )
            )
        );
    }

    private static P.Shape CreateTitleShapeWithTheme(string title)
    {
        return new P.Shape(
            new P.NonVisualShapeProperties(
                new P.NonVisualDrawingProperties() { Id = 2, Name = "Title" },
                new P.NonVisualShapeDrawingProperties(new D.ShapeLocks() { NoGrouping = true }),
                new P.ApplicationNonVisualDrawingProperties(
                    new P.PlaceholderShape() { Type = P.PlaceholderValues.Title }
                )
            ),
            new P.ShapeProperties(
                new D.Transform2D(
                    new D.Offset() { X = 914400L, Y = 1828800L },
                    new D.Extents() { Cx = 10297200L, Cy = 1368296L }
                )
            ),
            new P.TextBody(
                new D.BodyProperties() { Anchor = D.TextAnchoringTypeValues.Center },
                new D.ListStyle(),
                new D.Paragraph(
                    new D.ParagraphProperties() { Alignment = D.TextAlignmentTypeValues.Center },
                    new D.Run(
                        new D.RunProperties(
                            new D.SolidFill(new D.SchemeColor() { Val = D.SchemeColorValues.Text1 })
                        )
                        {
                            FontSize = 4400,
                            Bold = true,
                        },
                        new D.Text(title)
                    )
                )
            )
        );
    }

    private static P.Shape CreateSubtitleShape(string subtitle)
    {
        return new P.Shape(
            new P.NonVisualShapeProperties(
                new P.NonVisualDrawingProperties() { Id = 3, Name = "Subtitle" },
                new P.NonVisualShapeDrawingProperties(new D.ShapeLocks() { NoGrouping = true }),
                new P.ApplicationNonVisualDrawingProperties(
                    new P.PlaceholderShape() { Type = P.PlaceholderValues.SubTitle }
                )
            ),
            new P.ShapeProperties(
                new D.Transform2D(
                    new D.Offset() { X = 914400L, Y = 3429000L },
                    new D.Extents() { Cx = 10297200L, Cy = 1000000L }
                )
            ),
            new P.TextBody(
                new D.BodyProperties() { Anchor = D.TextAnchoringTypeValues.Center },
                new D.ListStyle(),
                new D.Paragraph(
                    new D.ParagraphProperties() { Alignment = D.TextAlignmentTypeValues.Center },
                    new D.Run(new D.RunProperties() { FontSize = 2800 }, new D.Text(subtitle))
                )
            )
        );
    }

    private static P.Shape CreateSubtitleShapeWithTheme(string subtitle)
    {
        return new P.Shape(
            new P.NonVisualShapeProperties(
                new P.NonVisualDrawingProperties() { Id = 3, Name = "Subtitle" },
                new P.NonVisualShapeDrawingProperties(new D.ShapeLocks() { NoGrouping = true }),
                new P.ApplicationNonVisualDrawingProperties(
                    new P.PlaceholderShape() { Type = P.PlaceholderValues.SubTitle }
                )
            ),
            new P.ShapeProperties(
                new D.Transform2D(
                    new D.Offset() { X = 914400L, Y = 3429000L },
                    new D.Extents() { Cx = 10297200L, Cy = 1000000L }
                )
            ),
            new P.TextBody(
                new D.BodyProperties() { Anchor = D.TextAnchoringTypeValues.Center },
                new D.ListStyle(),
                new D.Paragraph(
                    new D.ParagraphProperties() { Alignment = D.TextAlignmentTypeValues.Center },
                    new D.Run(
                        new D.RunProperties(
                            new D.SolidFill(new D.SchemeColor() { Val = D.SchemeColorValues.Text2 })
                        )
                        {
                            FontSize = 2800,
                        },
                        new D.Text(subtitle)
                    )
                )
            )
        );
    }

    private static P.Shape CreateLyricsShape(string lyrics)
    {
        return new P.Shape(
            new P.NonVisualShapeProperties(
                new P.NonVisualDrawingProperties() { Id = 2, Name = "Lyrics" },
                new P.NonVisualShapeDrawingProperties(new D.ShapeLocks() { NoGrouping = true }),
                new P.ApplicationNonVisualDrawingProperties()
            ),
            new P.ShapeProperties(
                new D.Transform2D(
                    new D.Offset() { X = 914400L, Y = 1000000L },
                    new D.Extents() { Cx = 10297200L, Cy = 4858000L }
                )
            ),
            CreateLyricTextBody(lyrics)
        );
    }

    private static P.Shape CreateLyricsShapeWithTheme(string lyrics)
    {
        return new P.Shape(
            new P.NonVisualShapeProperties(
                new P.NonVisualDrawingProperties() { Id = 2, Name = "Lyrics" },
                new P.NonVisualShapeDrawingProperties(new D.ShapeLocks() { NoGrouping = true }),
                new P.ApplicationNonVisualDrawingProperties(
                    new P.PlaceholderShape() { Type = P.PlaceholderValues.Body }
                )
            ),
            new P.ShapeProperties(
                new D.Transform2D(
                    new D.Offset() { X = 914400L, Y = 1000000L },
                    new D.Extents() { Cx = 10297200L, Cy = 4858000L }
                )
            ),
            CreateLyricTextBodyWithTheme(lyrics)
        );
    }

    private static P.TextBody CreateLyricTextBody(string lyrics)
    {
        var textBody = new P.TextBody(
            new D.BodyProperties()
            {
                Anchor = D.TextAnchoringTypeValues.Center,
                Wrap = D.TextWrappingValues.Square,
            },
            new D.ListStyle()
        );

        var lines = lyrics.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            textBody.Append(
                new D.Paragraph(
                    new D.ParagraphProperties() { Alignment = D.TextAlignmentTypeValues.Center },
                    new D.Run(new D.RunProperties() { FontSize = 3200 }, new D.Text(line.Trim()))
                )
            );
        }

        // If no lines, add empty paragraph
        if (lines.Length == 0)
        {
            textBody.Append(new D.Paragraph(new D.Run(new D.Text(""))));
        }

        return textBody;
    }

    private static P.TextBody CreateLyricTextBodyWithTheme(string lyrics)
    {
        var textBody = new P.TextBody(
            new D.BodyProperties()
            {
                Anchor = D.TextAnchoringTypeValues.Center,
                Wrap = D.TextWrappingValues.Square,
            },
            new D.ListStyle()
        );

        var lines = lyrics.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            textBody.Append(
                new D.Paragraph(
                    new D.ParagraphProperties() { Alignment = D.TextAlignmentTypeValues.Center },
                    new D.Run(
                        new D.RunProperties(
                            new D.SolidFill(new D.SchemeColor() { Val = D.SchemeColorValues.Text1 })
                        )
                        {
                            FontSize = 3200,
                        },
                        new D.Text(line.Trim())
                    )
                )
            );
        }

        // If no lines, add empty paragraph
        if (lines.Length == 0)
        {
            textBody.Append(
                new D.Paragraph(
                    new D.Run(
                        new D.RunProperties(
                            new D.SolidFill(new D.SchemeColor() { Val = D.SchemeColorValues.Text1 })
                        ),
                        new D.Text("")
                    )
                )
            );
        }

        return textBody;
    }
}
