namespace JsonEditor.Core.Models;

public sealed class JsonTreeNode
{
    public required string Label { get; init; }

    public List<JsonTreeNode> Children { get; } = new();
}
