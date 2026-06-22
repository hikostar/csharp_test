using JsonEditor.Core.Models;

namespace JsonEditor.Core.Services;

/// <summary>
/// JSON の検証を行うサービスのインターフェース
/// </summary>
public interface IJsonValidationService
{
    /// <summary>
    /// テキストが有効な JSON 形式であるかを検証します
    /// </summary>
    /// <param name="text">検証対象のテキスト</param>
    /// <returns>検証結果</returns>
    JsonValidationResult Validate(string text);
}
