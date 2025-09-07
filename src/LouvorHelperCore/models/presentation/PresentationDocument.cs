using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using LouvorHelperCore.Utils;
using D = DocumentFormat.OpenXml.Drawing;
using P = DocumentFormat.OpenXml.Presentation;

namespace LouvorHelperCore.Models.Presentation;

/// <summary>
/// Main presentation document class - handles creating PowerPoint presentations from music data
/// </summary>
internal class PresentationDocument
{
    private readonly Music _music;
    private readonly List<Slide> _slides;
    private string? _templatePath;

    public PresentationDocument(Music music)
    {
        _music = music;
        _slides = new List<Slide>();
        GenerateSlidesFromMusic();
    }

    /// <summary>
    /// Sets the PowerPoint template to use for the presentation
    /// </summary>
    /// <param name="templatePath">Path to the PowerPoint template file</param>
    /// <returns>This PresentationDocument instance for method chaining</returns>
    public PresentationDocument SetTemplate(string templatePath)
    {
        if (TemplateManager.IsValidTemplate(templatePath))
            _templatePath = templatePath;
        return this;
    }

    /// <summary>
    /// Adds a slide to the presentation
    /// </summary>
    /// <param name="slide">The slide to add</param>
    public void AddSlide(Slide slide)
    {
        _slides.Add(slide);
    }

    /// <summary>
    /// Inserts a slide at the specified position
    /// </summary>
    /// <param name="index">The position to insert the slide</param>
    /// <param name="slide">The slide to insert</param>
    public void InsertSlide(int index, Slide slide)
    {
        if (index < 0 || index > _slides.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        _slides.Insert(index, slide);
    }

    /// <summary>
    /// Removes a slide at the specified position
    /// </summary>
    /// <param name="index">The position of the slide to remove</param>
    public void RemoveSlide(int index)
    {
        if (index >= 0 && index < _slides.Count)
        {
            _slides.RemoveAt(index);
        }
    }

    /// <summary>
    /// Gets the current slides in the presentation
    /// </summary>
    public IReadOnlyList<Slide> Slides => _slides.AsReadOnly();

    /// <summary>
    /// Saves the presentation to the specified file path
    /// </summary>
    /// <param name="filepath">The path where the presentation will be saved</param>
    public void Save(string filepath)
    {
        try
        {
            if (!string.IsNullOrEmpty(_templatePath))
            {
                CreatePresentationFromTemplate(filepath);
            }
            else
            {
                CreatePresentationFromScratch(filepath);
            }
        }
        catch (Exception ex)
        {
            Notify.Error($"Erro ao criar apresentação para {_music.Title}: {ex.Message}");
            throw;
        }
    }

    private void GenerateSlidesFromMusic()
    {
        // Add title slide
        _slides.Add(Slide.CreateTitleSlide(_music.Title, _music.Artist));

        // Add lyrics slides
        if (!string.IsNullOrEmpty(_music.Lyrics))
        {
            var lyricSections = LyricsSplitter.SplitLyrics(_music.Lyrics);
            foreach (var section in lyricSections)
            {
                _slides.Add(Slide.CreateLyricsSlide(section));
            }
        }
    }

    private void CreatePresentationFromTemplate(string filepath)
    {
        using var presentationDoc = TemplateManager.CreateFromTemplate(_templatePath!, filepath);
        var presentationPart = presentationDoc.PresentationPart!;

        // Clear existing slides but keep master and layouts
        ClearExistingSlides(presentationPart);

        // Get or create layout manager
        var layoutManager = new SlideLayoutManager(presentationPart);

        // Add our slides with proper layout references
        AddSlidesToPresentationWithLayouts(presentationPart, layoutManager);

        // Ensure required elements are present
        EnsureRequiredPresentationElements(presentationPart);

        presentationPart.Presentation.Save();
    }

    private void CreatePresentationFromScratch(string filepath)
    {
        using var presentationDoc = DocumentFormat.OpenXml.Packaging.PresentationDocument.Create(
            filepath,
            PresentationDocumentType.Presentation
        );

        var presentationPart = presentationDoc.AddPresentationPart();
        presentationPart.Presentation = new P.Presentation();

        // Create slide master and layouts
        var slideMasterPart = CreateSlideMaster(presentationPart);
        var layoutManager = new SlideLayoutManager(presentationPart);
        layoutManager.CreateDefaultLayouts(slideMasterPart);

        // Add slides with proper layout references
        AddSlidesToPresentationWithLayouts(presentationPart, layoutManager);

        // Set slide size (16:9 widescreen)
        presentationPart.Presentation.SlideSize = new P.SlideSize()
        {
            Cx = 12192000, // 16:9 width
            Cy = 6858000, // 16:9 height
            Type = P.SlideSizeValues.Screen16x9,
        };

        // Ensure required elements are present
        EnsureRequiredPresentationElements(presentationPart);

        presentationPart.Presentation.Save();
    }

    private void AddSlidesToPresentationWithLayouts(
        PresentationPart presentationPart,
        SlideLayoutManager layoutManager
    )
    {
        var slideIdList = new P.SlideIdList();
        uint slideId = 256;
        int relationshipId = GetNextRelationshipId(presentationPart);

        foreach (var slide in _slides)
        {
            var slidePart = presentationPart.AddNewPart<SlidePart>($"rId{relationshipId}");

            // Get appropriate layout
            var layoutPart = layoutManager.GetLayout(slide.Layout);

            // Create slide with layout reference
            slidePart.Slide = SlideFactory.CreateSlideWithLayout(slide, layoutPart);

            // Add relationship to layout
            slidePart.AddPart(layoutPart);

            slidePart.Slide.Save();

            slideIdList.Append(
                new P.SlideId()
                {
                    Id = (UInt32Value)slideId++,
                    RelationshipId = $"rId{relationshipId++}",
                }
            );
        }

        presentationPart.Presentation.SlideIdList = slideIdList;
    }

    private void AddSlidesToPresentation(PresentationPart presentationPart)
    {
        var slideIdList = new P.SlideIdList();
        uint slideId = 256;
        int relationshipId = GetNextRelationshipId(presentationPart);

        foreach (var slide in _slides)
        {
            var slidePart = presentationPart.AddNewPart<SlidePart>($"rId{relationshipId}");
            slidePart.Slide = SlideFactory.CreateSlide(slide);
            slidePart.Slide.Save();

            slideIdList.Append(
                new P.SlideId()
                {
                    Id = (UInt32Value)slideId++,
                    RelationshipId = $"rId{relationshipId++}",
                }
            );
        }

        presentationPart.Presentation.SlideIdList = slideIdList;
    }

    private static void ClearExistingSlides(PresentationPart presentationPart)
    {
        // Remove existing slides but keep the structure
        var slideIdList = presentationPart.Presentation.SlideIdList;
        if (slideIdList != null)
        {
            var slideIds = slideIdList.Elements<P.SlideId>().ToList();
            foreach (var slideId in slideIds)
            {
                var slidePart = (SlidePart)presentationPart.GetPartById(slideId.RelationshipId!);
                presentationPart.DeletePart(slidePart);
                slideId.Remove();
            }
        }
    }

    private static void EnsureRequiredPresentationElements(PresentationPart presentationPart)
    {
        var presentation = presentationPart.Presentation;

        // Ensure SlideIdList exists
        if (presentation.SlideIdList == null)
        {
            presentation.SlideIdList = new P.SlideIdList();
        }

        // Ensure SlideMasterIdList exists
        if (presentation.SlideMasterIdList == null)
        {
            presentation.SlideMasterIdList = new P.SlideMasterIdList();
        }

        // Ensure slide size is set
        if (presentation.SlideSize == null)
        {
            presentation.SlideSize = new P.SlideSize()
            {
                Cx = 12192000,
                Cy = 6858000,
                Type = P.SlideSizeValues.Screen16x9,
            };
        }

        // Ensure notes size is set
        if (presentation.NotesSize == null)
        {
            presentation.NotesSize = new P.NotesSize() { Cx = 12192000, Cy = 9144000 };
        }

        // Ensure default text styles exist
        if (presentation.DefaultTextStyle == null)
        {
            presentation.DefaultTextStyle = new P.DefaultTextStyle();
        }
    }

    private static int GetNextRelationshipId(PresentationPart presentationPart)
    {
        // Find the highest relationship ID and return next available
        int maxId = 1;
        foreach (var part in presentationPart.Parts)
        {
            if (part.RelationshipId.StartsWith("rId"))
            {
                if (int.TryParse(part.RelationshipId.Substring(3), out int id))
                {
                    maxId = Math.Max(maxId, id);
                }
            }
        }
        return maxId + 1;
    }

    private SlideMasterPart CreateSlideMaster(PresentationPart presentationPart)
    {
        var slideMasterPart = presentationPart.AddNewPart<SlideMasterPart>("rId1");

        // Create theme part
        var themePart = slideMasterPart.AddNewPart<ThemePart>("rId1");
        themePart.Theme = CreateDefaultTheme();

        slideMasterPart.SlideMaster = new P.SlideMaster(
            new P.CommonSlideData(
                new P.ShapeTree(
                    new P.NonVisualGroupShapeProperties(
                        new P.NonVisualDrawingProperties() { Id = 1, Name = "" },
                        new P.NonVisualGroupShapeProperties(),
                        new P.ApplicationNonVisualDrawingProperties()
                    ),
                    new P.GroupShapeProperties(new D.TransformGroup()),
                    // Add title placeholder
                    CreateMasterTitlePlaceholder(),
                    // Add content placeholder
                    CreateMasterContentPlaceholder()
                )
            ),
            new P.ColorMap()
            {
                Background1 = D.ColorSchemeIndexValues.Light1,
                Text1 = D.ColorSchemeIndexValues.Dark1,
                Background2 = D.ColorSchemeIndexValues.Light2,
                Text2 = D.ColorSchemeIndexValues.Dark2,
                Accent1 = D.ColorSchemeIndexValues.Accent1,
                Accent2 = D.ColorSchemeIndexValues.Accent2,
                Accent3 = D.ColorSchemeIndexValues.Accent3,
                Accent4 = D.ColorSchemeIndexValues.Accent4,
                Accent5 = D.ColorSchemeIndexValues.Accent5,
                Accent6 = D.ColorSchemeIndexValues.Accent6,
                Hyperlink = D.ColorSchemeIndexValues.Hyperlink,
                FollowedHyperlink = D.ColorSchemeIndexValues.FollowedHyperlink,
            }
        );

        presentationPart.Presentation.SlideMasterIdList = new P.SlideMasterIdList(
            new P.SlideMasterId() { Id = (UInt32Value)2147483648U, RelationshipId = "rId1" }
        );

        return slideMasterPart;
    }

    private static P.Shape CreateMasterTitlePlaceholder()
    {
        return new P.Shape(
            new P.NonVisualShapeProperties(
                new P.NonVisualDrawingProperties() { Id = 2, Name = "Title Placeholder 1" },
                new P.NonVisualShapeDrawingProperties(new D.ShapeLocks() { NoGrouping = true }),
                new P.ApplicationNonVisualDrawingProperties(
                    new P.PlaceholderShape() { Type = P.PlaceholderValues.Title }
                )
            ),
            new P.ShapeProperties(),
            new P.TextBody(
                new D.BodyProperties(),
                new D.ListStyle(),
                new D.Paragraph(new D.EndParagraphRunProperties() { Language = "en-US" })
            )
        );
    }

    private static P.Shape CreateMasterContentPlaceholder()
    {
        return new P.Shape(
            new P.NonVisualShapeProperties(
                new P.NonVisualDrawingProperties() { Id = 3, Name = "Content Placeholder 2" },
                new P.NonVisualShapeDrawingProperties(new D.ShapeLocks() { NoGrouping = true }),
                new P.ApplicationNonVisualDrawingProperties(
                    new P.PlaceholderShape() { Type = P.PlaceholderValues.Body }
                )
            ),
            new P.ShapeProperties(),
            new P.TextBody(
                new D.BodyProperties(),
                new D.ListStyle(),
                new D.Paragraph(new D.EndParagraphRunProperties() { Language = "en-US" })
            )
        );
    }

    private static D.Theme CreateDefaultTheme()
    {
        return new D.Theme(
            new D.ThemeElements(
                new D.ColorScheme(
                    new D.Dark1Color(new D.SystemColor() { Val = D.SystemColorValues.WindowText }),
                    new D.Light1Color(new D.SystemColor() { Val = D.SystemColorValues.Window }),
                    new D.Dark2Color(new D.RgbColorModelHex() { Val = "1F497D" }),
                    new D.Light2Color(new D.RgbColorModelHex() { Val = "EEECE1" }),
                    new D.Accent1Color(new D.RgbColorModelHex() { Val = "4F81BD" }),
                    new D.Accent2Color(new D.RgbColorModelHex() { Val = "F79646" }),
                    new D.Accent3Color(new D.RgbColorModelHex() { Val = "9BBB59" }),
                    new D.Accent4Color(new D.RgbColorModelHex() { Val = "8064A2" }),
                    new D.Accent5Color(new D.RgbColorModelHex() { Val = "4BACC6" }),
                    new D.Accent6Color(new D.RgbColorModelHex() { Val = "F366A7" }),
                    new D.Hyperlink(new D.RgbColorModelHex() { Val = "0000FF" }),
                    new D.FollowedHyperlinkColor(new D.RgbColorModelHex() { Val = "800080" })
                )
                {
                    Name = "Office",
                },
                new D.FontScheme(
                    new D.MajorFont(
                        new D.LatinFont() { Typeface = "Calibri Light" },
                        new D.EastAsianFont() { Typeface = "" },
                        new D.ComplexScriptFont() { Typeface = "" }
                    ),
                    new D.MinorFont(
                        new D.LatinFont() { Typeface = "Calibri" },
                        new D.EastAsianFont() { Typeface = "" },
                        new D.ComplexScriptFont() { Typeface = "" }
                    )
                )
                {
                    Name = "Office",
                },
                new D.FormatScheme(
                    new D.FillStyleList(
                        new D.SolidFill(new D.SchemeColor() { Val = D.SchemeColorValues.PhColor }),
                        new D.GradientFill(
                            new D.GradientStopList(
                                new D.GradientStop(
                                    new D.SchemeColor() { Val = D.SchemeColorValues.PhColor }
                                )
                                {
                                    Position = 0,
                                },
                                new D.GradientStop(
                                    new D.SchemeColor() { Val = D.SchemeColorValues.PhColor }
                                )
                                {
                                    Position = 100000,
                                }
                            ),
                            new D.LinearGradientFill() { Angle = 5400000, Scaled = true }
                        ),
                        new D.GradientFill(
                            new D.GradientStopList(
                                new D.GradientStop(
                                    new D.SchemeColor() { Val = D.SchemeColorValues.PhColor }
                                )
                                {
                                    Position = 0,
                                },
                                new D.GradientStop(
                                    new D.SchemeColor() { Val = D.SchemeColorValues.PhColor }
                                )
                                {
                                    Position = 100000,
                                }
                            ),
                            new D.LinearGradientFill() { Angle = 5400000, Scaled = true }
                        )
                    ),
                    new D.LineStyleList(
                        new D.Outline(
                            new D.SolidFill(
                                new D.SchemeColor() { Val = D.SchemeColorValues.PhColor }
                            ),
                            new D.PresetDash() { Val = D.PresetLineDashValues.Solid }
                        )
                        {
                            Width = 9525,
                        },
                        new D.Outline(
                            new D.SolidFill(
                                new D.SchemeColor() { Val = D.SchemeColorValues.PhColor }
                            )
                        )
                        {
                            Width = 25400,
                        },
                        new D.Outline(
                            new D.SolidFill(
                                new D.SchemeColor() { Val = D.SchemeColorValues.PhColor }
                            )
                        )
                        {
                            Width = 38100,
                        }
                    ),
                    new D.EffectStyleList(
                        new D.EffectStyle(
                            new D.EffectList(
                                new D.OuterShadow(
                                    new D.RgbColorModelHex(new D.Alpha() { Val = 38000 })
                                    {
                                        Val = "000000",
                                    }
                                )
                                {
                                    BlurRadius = 40000L,
                                    Distance = 20000L,
                                    Direction = 5400000,
                                    RotateWithShape = false,
                                }
                            )
                        ),
                        new D.EffectStyle(
                            new D.EffectList(
                                new D.OuterShadow(
                                    new D.RgbColorModelHex(new D.Alpha() { Val = 35000 })
                                    {
                                        Val = "000000",
                                    }
                                )
                                {
                                    BlurRadius = 40000L,
                                    Distance = 23000L,
                                    Direction = 5400000,
                                    RotateWithShape = false,
                                }
                            )
                        ),
                        new D.EffectStyle(
                            new D.EffectList(
                                new D.OuterShadow(
                                    new D.RgbColorModelHex(new D.Alpha() { Val = 35000 })
                                    {
                                        Val = "000000",
                                    }
                                )
                                {
                                    BlurRadius = 40000L,
                                    Distance = 23000L,
                                    Direction = 5400000,
                                    RotateWithShape = false,
                                }
                            )
                        )
                    ),
                    new D.BackgroundFillStyleList(
                        new D.SolidFill(new D.SchemeColor() { Val = D.SchemeColorValues.PhColor }),
                        new D.GradientFill(
                            new D.GradientStopList(
                                new D.GradientStop(
                                    new D.SchemeColor() { Val = D.SchemeColorValues.PhColor }
                                )
                                {
                                    Position = 0,
                                },
                                new D.GradientStop(
                                    new D.SchemeColor() { Val = D.SchemeColorValues.PhColor }
                                )
                                {
                                    Position = 100000,
                                }
                            ),
                            new D.LinearGradientFill() { Angle = 5400000, Scaled = true }
                        ),
                        new D.GradientFill(
                            new D.GradientStopList(
                                new D.GradientStop(
                                    new D.SchemeColor() { Val = D.SchemeColorValues.PhColor }
                                )
                                {
                                    Position = 0,
                                },
                                new D.GradientStop(
                                    new D.SchemeColor() { Val = D.SchemeColorValues.PhColor }
                                )
                                {
                                    Position = 100000,
                                }
                            ),
                            new D.LinearGradientFill() { Angle = 5400000, Scaled = true }
                        )
                    )
                )
                {
                    Name = "Office",
                }
            )
        )
        {
            Name = "Office Theme",
        };
    }
}
