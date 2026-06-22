using JsonEditor.Core.Models;
using JsonEditor.Core.Services;

namespace JsonEditor.App.Tests;

public class SearchReplaceServiceTests
{
    [Fact]
    public void CountMatches_WorksForCaseInsensitivePlainSearch()
    {
        var sut = new SearchReplaceService();
        var options = new SearchOptions
        {
            SearchText = "json",
            MatchCase = false,
            UseRegex = false
        };

        var count = sut.CountMatches("Json JSON jSoN", options);

        Assert.Equal(3, count);
    }

    [Fact]
    public void ReplaceAll_WorksForRegex()
    {
        var sut = new SearchReplaceService();
        var options = new SearchOptions
        {
            SearchText = "\\d+",
            ReplaceText = "#",
            UseRegex = true,
            MatchCase = true
        };

        var replaced = sut.ReplaceAll("a1 b22 c333", options);

        Assert.Equal("a# b# c#", replaced);
    }

    [Fact]
    public void FindNextMatch_WrapsToBeginning_WhenNoFurtherMatch()
    {
        var sut = new SearchReplaceService();
        var options = new SearchOptions
        {
            SearchText = "abc",
            MatchCase = true,
            UseRegex = false
        };

        var match = sut.FindNextMatch("abc xx abc", options, 8);

        Assert.NotNull(match);
        Assert.Equal(0, match!.Value.Start);
        Assert.Equal(3, match.Value.Length);
    }

    [Fact]
    public void FindPreviousMatch_WrapsToEnd_WhenNoPreviousMatch()
    {
        var sut = new SearchReplaceService();
        var options = new SearchOptions
        {
            SearchText = "abc",
            MatchCase = true,
            UseRegex = false
        };

        var match = sut.FindPreviousMatch("abc xx abc", options, 0);

        Assert.NotNull(match);
        Assert.Equal(7, match!.Value.Start);
        Assert.Equal(3, match.Value.Length);
    }

    [Fact]
    public void BuildReplacePreview_CreatesPreviewItems_ForRegex()
    {
        var sut = new SearchReplaceService();
        var options = new SearchOptions
        {
            SearchText = "item-(\\d+)",
            ReplaceText = "ID:$1",
            MatchCase = true,
            UseRegex = true
        };

        var previews = sut.BuildReplacePreview("item-12, item-3", options, 10, out var total);

        Assert.Equal(2, total);
        Assert.Equal(2, previews.Count);
        Assert.Equal("ID:12", previews[0].ReplacementText);
        Assert.Equal("ID:3", previews[1].ReplacementText);
    }
}
