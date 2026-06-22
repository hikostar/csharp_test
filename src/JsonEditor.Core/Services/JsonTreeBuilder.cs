using System.Text.Json;
using JsonEditor.Core.Models;

namespace JsonEditor.Core.Services;

public sealed class JsonTreeBuilder
{
    public JsonTreeNode? Build(string jsonText)
    {
        if (string.IsNullOrWhiteSpace(jsonText))
        {
            return null;
        }

        using var document = JsonDocument.Parse(jsonText);
        return BuildNode("$", document.RootElement);
    }

    private static JsonTreeNode BuildNode(string label, JsonElement element)
    {
        var node = new JsonTreeNode { Label = FormatLabel(label, element) };

        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    node.Children.Add(BuildNode(property.Name, property.Value));
                }
                break;
            case JsonValueKind.Array:
                var index = 0;
                foreach (var item in element.EnumerateArray())
                {
                    node.Children.Add(BuildNode($"[{index}]", item));
                    index++;
                }
                break;
        }

        return node;
    }

    private static string FormatLabel(string key, JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => $"{key}: {{}}",
            JsonValueKind.Array => $"{key}: []",
            JsonValueKind.String => $"{key}: \"{element.GetString()}\"",
            JsonValueKind.Number => $"{key}: {element.GetRawText()}",
            JsonValueKind.True => $"{key}: true",
            JsonValueKind.False => $"{key}: false",
            JsonValueKind.Null => $"{key}: null",
            _ => $"{key}: {element.GetRawText()}"
        };
    }
}
