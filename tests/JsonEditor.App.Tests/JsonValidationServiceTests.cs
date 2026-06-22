using JsonEditor.Core.Services;

namespace JsonEditor.App.Tests;

public class JsonValidationServiceTests
{
    [Fact]
    public void Validate_ReturnsValid_ForCorrectJson()
    {
        var sut = new JsonValidationService();

        var result = sut.Validate("{\"name\":\"copilot\",\"ok\":true}");

        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Validate_ReturnsInvalid_ForBrokenJson()
    {
        var sut = new JsonValidationService();

        var result = sut.Validate("{\"name\":\"copilot\"");

        Assert.False(result.IsValid);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
    }
}
