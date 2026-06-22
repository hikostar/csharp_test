using JsonEditor.Core.Models;

namespace JsonEditor.Core.Services;

/// <summary>
/// JSON テキストを UI ツリー構造に変換するサービスのインターフェース
/// </summary>
public interface IJsonTreeBuilder
{
    /// <summary>
    /// JSON テキストを TreeNode 構造に変換します
    /// </summary>
    /// <param name="jsonText">JSON テキスト</param>
    /// <returns>ルートの TreeNode（失敗時は null）</returns>
    JsonTreeNode? Build(string jsonText);
}
