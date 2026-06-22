using System.Text.RegularExpressions;
using JsonEditor.Core.Models;

namespace JsonEditor.Core.Services;

public sealed class SearchReplaceService
{
    public (int Start, int Length)? FindNextMatch(string text, SearchOptions options, int fromIndex)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(options.SearchText))
        {
            return null;
        }

        var startIndex = Math.Clamp(fromIndex, 0, text.Length);
        if (options.UseRegex)
        {
            var regex = BuildRegex(options);
            var match = regex.Match(text, startIndex);
            if (match.Success)
            {
                return (match.Index, Math.Max(1, match.Length));
            }

            var wrapped = regex.Match(text);
            return wrapped.Success ? (wrapped.Index, Math.Max(1, wrapped.Length)) : null;
        }

        var comparison = options.MatchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        var found = text.IndexOf(options.SearchText, startIndex, comparison);
        if (found >= 0)
        {
            return (found, options.SearchText.Length);
        }

        found = text.IndexOf(options.SearchText, 0, comparison);
        return found >= 0 ? (found, options.SearchText.Length) : null;
    }

    public (int Start, int Length)? FindPreviousMatch(string text, SearchOptions options, int fromIndex)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(options.SearchText))
        {
            return null;
        }

        var clamped = Math.Clamp(fromIndex, 0, text.Length);
        if (options.UseRegex)
        {
            var regex = BuildRegex(options);
            var matches = regex.Matches(text);
            if (matches.Count == 0)
            {
                return null;
            }

            Match? candidate = null;
            foreach (Match match in matches)
            {
                if (match.Index < clamped)
                {
                    candidate = match;
                    continue;
                }

                break;
            }

            var selected = candidate ?? matches[^1];
            return (selected.Index, Math.Max(1, selected.Length));
        }

        var comparison = options.MatchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        var seekFrom = clamped <= 0 ? text.Length - 1 : clamped - 1;
        var found = text.LastIndexOf(options.SearchText, seekFrom, comparison);
        if (found >= 0)
        {
            return (found, options.SearchText.Length);
        }

        found = text.LastIndexOf(options.SearchText, text.Length - 1, comparison);
        return found >= 0 ? (found, options.SearchText.Length) : null;
    }

    public IReadOnlyList<ReplacePreviewItem> BuildReplacePreview(string text, SearchOptions options, int maxItems, out int totalMatches)
    {
        totalMatches = 0;
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(options.SearchText) || maxItems <= 0)
        {
            return [];
        }

        var previews = new List<ReplacePreviewItem>();
        if (options.UseRegex)
        {
            var regex = BuildRegex(options);
            foreach (Match match in regex.Matches(text))
            {
                totalMatches++;
                if (previews.Count >= maxItems)
                {
                    continue;
                }

                previews.Add(new ReplacePreviewItem
                {
                    Start = match.Index,
                    Length = Math.Max(1, match.Length),
                    OriginalText = match.Value,
                    ReplacementText = match.Result(options.ReplaceText)
                });
            }

            return previews;
        }

        var comparison = options.MatchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        var cursor = 0;
        while (cursor <= text.Length)
        {
            var found = text.IndexOf(options.SearchText, cursor, comparison);
            if (found < 0)
            {
                return previews;
            }

            totalMatches++;
            if (previews.Count < maxItems)
            {
                previews.Add(new ReplacePreviewItem
                {
                    Start = found,
                    Length = options.SearchText.Length,
                    OriginalText = text.Substring(found, options.SearchText.Length),
                    ReplacementText = options.ReplaceText
                });
            }

            cursor = found + Math.Max(1, options.SearchText.Length);
        }

        return previews;
    }

    public int CountMatches(string text, SearchOptions options)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(options.SearchText))
        {
            return 0;
        }

        if (options.UseRegex)
        {
            var regex = BuildRegex(options);
            return regex.Matches(text).Count;
        }

        var comparison = options.MatchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        var count = 0;
        var currentIndex = 0;

        while (true)
        {
            var found = text.IndexOf(options.SearchText, currentIndex, comparison);
            if (found < 0)
            {
                return count;
            }

            count++;
            currentIndex = found + Math.Max(1, options.SearchText.Length);
        }
    }

    public string ReplaceAll(string text, SearchOptions options)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(options.SearchText))
        {
            return text;
        }

        if (options.UseRegex)
        {
            var regex = BuildRegex(options);
            return regex.Replace(text, options.ReplaceText);
        }

        var comparison = options.MatchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        var cursor = 0;
        var result = new System.Text.StringBuilder(text.Length);

        while (true)
        {
            var found = text.IndexOf(options.SearchText, cursor, comparison);
            if (found < 0)
            {
                result.Append(text.AsSpan(cursor));
                return result.ToString();
            }

            result.Append(text.AsSpan(cursor, found - cursor));
            result.Append(options.ReplaceText);
            cursor = found + options.SearchText.Length;
        }
    }

    private static Regex BuildRegex(SearchOptions options)
    {
        var regexOptions = RegexOptions.Compiled | RegexOptions.Multiline;
        if (!options.MatchCase)
        {
            regexOptions |= RegexOptions.IgnoreCase;
        }

        return new Regex(options.SearchText, regexOptions, TimeSpan.FromMilliseconds(250));
    }
}
