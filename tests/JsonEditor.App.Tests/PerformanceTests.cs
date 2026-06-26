using System.Diagnostics;
using System.Text;
using JsonEditor.Core.Models;
using JsonEditor.Core.Services;

namespace JsonEditor.App.Tests;

public class PerformanceTests
{
    [Fact]
    [Trait("Category", "Performance")]
    public void JsonValidation_LargePayload_CompletesWithinBudget()
    {
        var sut = new JsonValidationService();
        var json = BuildLargeJson(8000);

        var sw = Stopwatch.StartNew();
        var result = sut.Validate(json);
        sw.Stop();

        Assert.True(result.IsValid);
        Assert.True(sw.ElapsedMilliseconds < 1500, $"Validation took {sw.ElapsedMilliseconds} ms");
    }

    [Fact]
    [Trait("Category", "Performance")]
    public void SearchCount_LargePayload_CompletesWithinBudget()
    {
        var sut = new SearchReplaceService();
        var text = BuildLargeText(120_000);
        var options = new SearchOptions
        {
            SearchText = "needle",
            MatchCase = true,
            UseRegex = false
        };

        var sw = Stopwatch.StartNew();
        var count = sut.CountMatches(text, options);
        sw.Stop();

        Assert.True(count > 0);
        Assert.True(sw.ElapsedMilliseconds < 1200, $"CountMatches took {sw.ElapsedMilliseconds} ms");
    }

    [Fact]
    [Trait("Category", "Performance")]
    public void ReplaceAll_LargePayload_CompletesWithinBudget()
    {
        var sut = new SearchReplaceService();
        var text = BuildLargeText(120_000);
        var options = new SearchOptions
        {
            SearchText = "needle",
            ReplaceText = "item",
            MatchCase = true,
            UseRegex = false
        };

        var sw = Stopwatch.StartNew();
        var replaced = sut.ReplaceAll(text, options);
        sw.Stop();

        Assert.DoesNotContain("needle", replaced, StringComparison.Ordinal);
        Assert.True(sw.ElapsedMilliseconds < 1500, $"ReplaceAll took {sw.ElapsedMilliseconds} ms");
    }

    [Fact]
    [Trait("Category", "Performance")]
    public void RegexPreview_LargePayload_CompletesWithinBudget()
    {
        var sut = new SearchReplaceService();
        var text = BuildLargeText(80_000);
        var options = new SearchOptions
        {
            SearchText = "need(le)",
            ReplaceText = "x$1",
            MatchCase = true,
            UseRegex = true
        };

        var sw = Stopwatch.StartNew();
        var preview = sut.BuildReplacePreview(text, options, 100, out var total);
        sw.Stop();

        Assert.True(total > 0);
        Assert.NotEmpty(preview);
        Assert.True(sw.ElapsedMilliseconds < 1500, $"BuildReplacePreview took {sw.ElapsedMilliseconds} ms");
    }

    private static string BuildLargeJson(int itemCount)
    {
        var sb = new StringBuilder(itemCount * 24);
        sb.Append("{\"items\":[");

        for (var i = 0; i < itemCount; i++)
        {
            if (i > 0)
            {
                sb.Append(',');
            }

            sb.Append("{\"id\":");
            sb.Append(i);
            sb.Append(",\"name\":\"item");
            sb.Append(i);
            sb.Append("\",\"flag\":");
            sb.Append(i % 2 == 0 ? "true" : "false");
            sb.Append('}');
        }

        sb.Append("]}");
        return sb.ToString();
    }

    private static string BuildLargeText(int repeat)
    {
        var sb = new StringBuilder(repeat * 16);
        for (var i = 0; i < repeat; i++)
        {
            sb.Append("needle-");
            sb.Append(i);
            sb.Append(' ');
        }

        return sb.ToString();
    }
}
