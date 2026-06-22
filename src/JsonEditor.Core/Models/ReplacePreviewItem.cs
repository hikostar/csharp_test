namespace JsonEditor.Core.Models;

public sealed class ReplacePreviewItem
{
    public required int Start { get; init; }
    public required int Length { get; init; }
    public required string OriginalText { get; init; }
    public required string ReplacementText { get; init; }
}
