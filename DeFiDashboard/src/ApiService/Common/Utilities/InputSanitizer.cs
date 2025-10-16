using Ganss.Xss;

namespace ApiService.Common.Utilities;

/// <summary>
/// Provides input sanitization to prevent XSS attacks
/// </summary>
public static class InputSanitizer
{
    private static readonly HtmlSanitizer Sanitizer = new();

    static InputSanitizer()
    {
        // Configure allowed tags and attributes
        Sanitizer.AllowedTags.Clear();
        Sanitizer.AllowedAttributes.Clear();

        // Allow basic formatting tags
        Sanitizer.AllowedTags.Add("p");
        Sanitizer.AllowedTags.Add("br");
        Sanitizer.AllowedTags.Add("strong");
        Sanitizer.AllowedTags.Add("em");
        Sanitizer.AllowedTags.Add("u");
    }

    /// <summary>
    /// Sanitizes HTML/text input to prevent XSS attacks
    /// </summary>
    /// <param name="input">Raw user input</param>
    /// <returns>Sanitized safe string</returns>
    public static string? Sanitize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        return Sanitizer.Sanitize(input);
    }

    /// <summary>
    /// Sanitizes plain text input (removes all HTML)
    /// </summary>
    /// <param name="input">Raw user input</param>
    /// <returns>Plain text without HTML</returns>
    public static string? SanitizePlainText(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        // Remove all HTML tags
        var sanitized = Sanitizer.Sanitize(input);

        // Further strip any remaining tags
        return System.Text.RegularExpressions.Regex.Replace(
            sanitized,
            "<.*?>",
            string.Empty
        );
    }
}
