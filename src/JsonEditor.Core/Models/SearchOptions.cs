namespace JsonEditor.Core.Models;

public sealed class SearchOptions
{
    public string SearchText { get; set; } = string.Empty;
    public string ReplaceText { get; set; } = string.Empty;
    public bool MatchCase { get; set; }
    public bool UseRegex { get; set; }
}
