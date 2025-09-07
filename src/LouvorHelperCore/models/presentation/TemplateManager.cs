using DocumentFormat.OpenXml.Packaging;
using P = DocumentFormat.OpenXml.Presentation;

namespace LouvorHelperCore.Models.Presentation;

/// <summary>
/// Handles PowerPoint template integration
/// </summary>
internal static class TemplateManager
{
    /// <summary>
    /// Creates a new presentation document from an existing template
    /// </summary>
    /// <param name="templatePath">Path to the template file</param>
    /// <param name="outputPath">Path where the new presentation will be saved</param>
    /// <returns>The created presentation document</returns>
    public static DocumentFormat.OpenXml.Packaging.PresentationDocument CreateFromTemplate(
        string templatePath,
        string outputPath
    )
    {
        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Template file not found: {templatePath}");
        }

        // Copy template to output location
        File.Copy(templatePath, outputPath, true);

        // Open the copied template
        return DocumentFormat.OpenXml.Packaging.PresentationDocument.Open(outputPath, true);
    }

    /// <summary>
    /// Applies theme settings from a template to an existing presentation
    /// </summary>
    /// <param name="presentationPart">The presentation part to apply the theme to</param>
    /// <param name="templatePath">Path to the template file</param>
    public static void ApplyTemplateTheme(PresentationPart presentationPart, string templatePath)
    {
        if (!File.Exists(templatePath))
        {
            return; // Silently continue if template doesn't exist
        }

        try
        {
            using var templateDoc = DocumentFormat.OpenXml.Packaging.PresentationDocument.Open(
                templatePath,
                false
            );
            var templatePresentationPart = templateDoc.PresentationPart;

            if (templatePresentationPart?.SlideMasterParts?.FirstOrDefault() != null)
            {
                var templateMaster = templatePresentationPart.SlideMasterParts.First();

                // Copy theme colors if available
                if (templateMaster.SlideMaster.ColorMap != null)
                {
                    var masterPart = presentationPart.SlideMasterParts.FirstOrDefault();
                    if (masterPart?.SlideMaster != null)
                    {
                        masterPart.SlideMaster.ColorMap = (P.ColorMap)
                            templateMaster.SlideMaster.ColorMap.CloneNode(true);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log error but don't fail the presentation creation
            System.Diagnostics.Debug.WriteLine($"Failed to apply template theme: {ex.Message}");
        }
    }

    /// <summary>
    /// Validates if a file is a valid PowerPoint template
    /// </summary>
    /// <param name="templatePath">Path to the template file</param>
    /// <returns>True if the file is a valid PowerPoint template</returns>
    public static bool IsValidTemplate(string templatePath)
    {
        if (!File.Exists(templatePath))
        {
            return false;
        }

        try
        {
            using var templateDoc = DocumentFormat.OpenXml.Packaging.PresentationDocument.Open(
                templatePath,
                false
            );
            return templateDoc.PresentationPart != null;
        }
        catch
        {
            return false;
        }
    }
}
