using JsonEditor.Core.Services;

namespace JsonEditor.App.Tests;

public class JsonTreeBuilderTests
{
    [Fact]
    public void Build_ReturnsRootNode_ForValidJson()
    {
        var sut = new JsonTreeBuilder();

        var node = sut.Build("{\"user\":{\"id\":1},\"tags\":[\"a\",\"b\"]}");

        Assert.NotNull(node);
        Assert.Equal("$: {}", node!.Label);
        Assert.Equal(2, node.Children.Count);
    }
}
