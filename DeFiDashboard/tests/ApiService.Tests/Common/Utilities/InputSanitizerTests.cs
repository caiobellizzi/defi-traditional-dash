using ApiService.Common.Utilities;
using FluentAssertions;

namespace ApiService.Tests.Common.Utilities;

public class InputSanitizerTests
{
    [Fact]
    public void Sanitize_RemovesScriptTags()
    {
        // Arrange
        var input = "<script>alert('XSS')</script>Hello World";

        // Act
        var result = InputSanitizer.Sanitize(input);

        // Assert
        result.Should().NotContain("<script>");
        result.Should().NotContain("alert");
    }

    [Fact]
    public void Sanitize_PreservesAllowedTags()
    {
        // Arrange
        var input = "<p>This is <strong>bold</strong> text</p>";

        // Act
        var result = InputSanitizer.Sanitize(input);

        // Assert
        result.Should().Contain("<p>");
        result.Should().Contain("<strong>");
    }

    [Fact]
    public void Sanitize_NullInput_ReturnsNull()
    {
        // Arrange
        string? input = null;

        // Act
        var result = InputSanitizer.Sanitize(input);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void SanitizePlainText_RemovesAllHtml()
    {
        // Arrange
        var input = "<p>Hello <strong>World</strong></p>";

        // Act
        var result = InputSanitizer.SanitizePlainText(input);

        // Assert
        result.Should().NotContain("<");
        result.Should().NotContain(">");
        result.Should().Contain("Hello");
        result.Should().Contain("World");
    }
}
