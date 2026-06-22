using JsonEditor.Core.Models;

namespace JsonEditor.Core.Services;

/// <summary>
/// テキスト内で検索と置換を行うサービスのインターフェース
/// </summary>
public interface ISearchReplaceService
{
    /// <summary>
    /// 次のマッチを検索します。見つからない場合は開始位置に戻ります
    /// </summary>
    /// <param name="text">対象のテキスト</param>
    /// <param name="options">検索オプション</param>
    /// <param name="fromIndex">検索開始位置</param>
    /// <returns>マッチ情報（見つからない場合は null）</returns>
    (int Start, int Length)? FindNextMatch(string text, SearchOptions options, int fromIndex);

    /// <summary>
    /// 前のマッチを検索します。見つからない場合は末尾に戻ります
    /// </summary>
    /// <param name="text">対象のテキスト</param>
    /// <param name="options">検索オプション</param>
    /// <param name="fromIndex">検索開始位置</param>
    /// <returns>マッチ情報（見つからない場合は null）</returns>
    (int Start, int Length)? FindPreviousMatch(string text, SearchOptions options, int fromIndex);

    /// <summary>
    /// 置換プレビューアイテムを生成します
    /// </summary>
    /// <param name="text">対象のテキスト</param>
    /// <param name="options">検索オプション</param>
    /// <param name="maxItems">最大アイテム数</param>
    /// <param name="totalMatches">総マッチ数（out パラメータ）</param>
    /// <returns>プレビューアイテムのリスト</returns>
    IReadOnlyList<ReplacePreviewItem> BuildReplacePreview(string text, SearchOptions options, int maxItems, out int totalMatches);

    /// <summary>
    /// 検索パターンに一致する個数をカウントします
    /// </summary>
    /// <param name="text">検索対象のテキスト</param>
    /// <param name="options">検索オプション</param>
    /// <returns>マッチ数</returns>
    int CountMatches(string text, SearchOptions options);

    /// <summary>
    /// すべての一致を置換します
    /// </summary>
    /// <param name="text">対象のテキスト</param>
    /// <param name="options">検索オプション</param>
    /// <returns>置換後のテキスト</returns>
    string ReplaceAll(string text, SearchOptions options);
}
