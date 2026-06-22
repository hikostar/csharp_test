namespace JsonEditor.Core.Models;

public sealed class AppSettings
{
    public string Theme { get; set; } = "Light";
    public int AutoSaveIntervalSeconds { get; set; } = 30;
    public bool UseRegexSearch { get; set; }
    public bool MatchCaseSearch { get; set; }
}
