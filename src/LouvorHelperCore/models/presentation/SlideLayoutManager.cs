using DocumentFormat.OpenXml.Packaging;
using D = DocumentFormat.OpenXml.Drawing;
using P = DocumentFormat.OpenXml.Presentation;

namespace LouvorHelperCore.Models.Presentation;

/// <summary>
/// Manages slide layouts and their creation
/// </summary>
internal class SlideLayoutManager
{
    private readonly PresentationPart _presentationPart;
    private readonly Dictionary<SlideLayoutType, SlideLayoutPart> _layouts;

    public SlideLayoutManager(PresentationPart presentationPart)
    {
        _presentationPart = presentationPart;
        _layouts = new Dictionary<SlideLayoutType, SlideLayoutPart>();

        // Try to use existing layouts from template first
        LoadExistingLayouts();
    }

    /// <summary>
    /// Creates default layouts for the presentation
    /// </summary>
    public void CreateDefaultLayouts(SlideMasterPart slideMasterPart)
    {
        // Only create layouts that don't already exist
        if (!_layouts.ContainsKey(SlideLayoutType.Title))
        {
            var titleLayout = slideMasterPart.AddNewPart<SlideLayoutPart>(
                $"rId{GetNextLayoutId()}"
            );
            titleLayout.SlideLayout = CreateTitleLayout();
            _layouts[SlideLayoutType.Title] = titleLayout;
        }

        if (!_layouts.ContainsKey(SlideLayoutType.Lyrics))
        {
            var lyricsLayout = slideMasterPart.AddNewPart<SlideLayoutPart>(
                $"rId{GetNextLayoutId()}"
            );
            lyricsLayout.SlideLayout = CreateLyricsLayout();
            _layouts[SlideLayoutType.Lyrics] = lyricsLayout;
        }

        // Ensure slide layout ID list exists on master
        EnsureSlideLayoutIdList(slideMasterPart);
    }

    /// <summary>
    /// Gets the layout part for the specified layout type
    /// </summary>
    public SlideLayoutPart GetLayout(SlideLayoutType layoutType)
    {
        return _layouts.TryGetValue(layoutType, out var layout)
            ? layout
            : _layouts.Values.FirstOrDefault() ?? CreateFallbackLayout();
    }

    private void LoadExistingLayouts()
    {
        // Load existing layouts from template if available
        var masterParts = _presentationPart.SlideMasterParts;
        foreach (var masterPart in masterParts)
        {
            foreach (var layoutPart in masterPart.SlideLayoutParts)
            {
                var slideLayout = layoutPart.SlideLayout;
                if (slideLayout?.Type != null)
                {
                    var layoutType = MapSlideLayoutType(slideLayout.Type.Value);
                    if (layoutType.HasValue && !_layouts.ContainsKey(layoutType.Value))
                    {
                        _layouts[layoutType.Value] = layoutPart;
                    }
                }
            }
        }
    }

    private SlideLayoutType? MapSlideLayoutType(P.SlideLayoutValues openXmlLayoutType)
    {
        if (openXmlLayoutType.Equals(P.SlideLayoutValues.Title))
            return SlideLayoutType.Title;

        if (openXmlLayoutType.Equals(P.SlideLayoutValues.TextAndObject))
            return SlideLayoutType.TitleAndContent;

        if (openXmlLayoutType.Equals(P.SlideLayoutValues.TwoColumnText))
            return SlideLayoutType.TwoColumn;

        if (openXmlLayoutType.Equals(P.SlideLayoutValues.Blank))
            return SlideLayoutType.Blank;

        return null;
    }

    private SlideLayoutPart CreateFallbackLayout()
    {
        // Create a basic layout if none exists
        var masterPart = _presentationPart.SlideMasterParts.FirstOrDefault();
        if (masterPart != null)
        {
            var fallbackLayout = masterPart.AddNewPart<SlideLayoutPart>($"rId{GetNextLayoutId()}");
            fallbackLayout.SlideLayout = CreateTitleLayout();
            _layouts[SlideLayoutType.Title] = fallbackLayout;
            return fallbackLayout;
        }

        throw new InvalidOperationException("No slide master available to create fallback layout");
    }

    private int GetNextLayoutId()
    {
        int maxId = 1;
        var masterParts = _presentationPart.SlideMasterParts;
        foreach (var masterPart in masterParts)
        {
            foreach (var part in masterPart.Parts)
            {
                if (part.RelationshipId.StartsWith("rId"))
                {
                    if (int.TryParse(part.RelationshipId.Substring(3), out int id))
                    {
                        maxId = Math.Max(maxId, id);
                    }
                }
            }
        }
        return maxId + 1;
    }

    private void EnsureSlideLayoutIdList(SlideMasterPart slideMasterPart)
    {
        if (slideMasterPart.SlideMaster.SlideLayoutIdList == null)
        {
            slideMasterPart.SlideMaster.SlideLayoutIdList = new P.SlideLayoutIdList();
        }

        var layoutIdList = slideMasterPart.SlideMaster.SlideLayoutIdList;
        uint layoutId = 2147483649; // Start after master ID

        foreach (var layoutPart in slideMasterPart.SlideLayoutParts)
        {
            var relationshipId = slideMasterPart.GetIdOfPart(layoutPart);

            // Check if this layout ID already exists
            bool exists = layoutIdList
                .Elements<P.SlideLayoutId>()
                .Any(lid => lid.RelationshipId == relationshipId);

            if (!exists)
            {
                layoutIdList.Append(
                    new P.SlideLayoutId() { Id = layoutId++, RelationshipId = relationshipId }
                );
            }
        }
    }

    private P.SlideLayout CreateTitleLayout()
    {
        return new P.SlideLayout(
            new P.CommonSlideData(
                new P.ShapeTree(
                    new P.NonVisualGroupShapeProperties(
                        new P.NonVisualDrawingProperties() { Id = 1, Name = "" },
                        new P.NonVisualGroupShapeDrawingProperties(),
                        new P.ApplicationNonVisualDrawingProperties()
                    ),
                    new P.GroupShapeProperties(new D.TransformGroup()),
                    // Title placeholder
                    CreateTitlePlaceholder(),
                    // Subtitle placeholder
                    CreateSubtitlePlaceholder()
                )
            ),
            new P.ColorMapOverride(new D.MasterColorMapping())
        )
        {
            Type = P.SlideLayoutValues.Title,
        };
    }

    private P.SlideLayout CreateLyricsLayout()
    {
        return new P.SlideLayout(
            new P.CommonSlideData(
                new P.ShapeTree(
                    new P.NonVisualGroupShapeProperties(
                        new P.NonVisualDrawingProperties() { Id = 1, Name = "" },
                        new P.NonVisualGroupShapeDrawingProperties(),
                        new P.ApplicationNonVisualDrawingProperties()
                    ),
                    new P.GroupShapeProperties(new D.TransformGroup()),
                    // Content placeholder for lyrics
                    CreateContentPlaceholder()
                )
            ),
            new P.ColorMapOverride(new D.MasterColorMapping())
        )
        {
            Type = P.SlideLayoutValues.Blank,
        };
    }

    private P.Shape CreateTitlePlaceholder()
    {
        return new P.Shape(
            new P.NonVisualShapeProperties(
                new P.NonVisualDrawingProperties() { Id = 2, Name = "Title 1" },
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
                    new D.EndParagraphRunProperties()
                    {
                        Language = "en-US",
                        FontSize = 4400,
                        Bold = true,
                    }
                )
            )
        );
    }

    private P.Shape CreateSubtitlePlaceholder()
    {
        return new P.Shape(
            new P.NonVisualShapeProperties(
                new P.NonVisualDrawingProperties() { Id = 3, Name = "Subtitle 2" },
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
                    new D.EndParagraphRunProperties() { Language = "en-US", FontSize = 2800 }
                )
            )
        );
    }

    private P.Shape CreateContentPlaceholder()
    {
        return new P.Shape(
            new P.NonVisualShapeProperties(
                new P.NonVisualDrawingProperties() { Id = 2, Name = "Content Placeholder 1" },
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
            new P.TextBody(
                new D.BodyProperties()
                {
                    Anchor = D.TextAnchoringTypeValues.Center,
                    Wrap = D.TextWrappingValues.Square,
                },
                new D.ListStyle(),
                new D.Paragraph(
                    new D.ParagraphProperties() { Alignment = D.TextAlignmentTypeValues.Center },
                    new D.EndParagraphRunProperties() { Language = "en-US", FontSize = 3200 }
                )
            )
        );
    }
}
