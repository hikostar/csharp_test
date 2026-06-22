using JsonEditor.Core.Models;

namespace JsonEditor.Core.Services;

/// <summary>
/// アプリケーション設定を永続化するサービスのインターフェース
/// </summary>
public interface IAppSettingsStore
{
    /// <summary>
    /// 設定を非同期で読み込みます
    /// </summary>
    /// <param name="filePath">設定ファイルのパス</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>読み込まれた設定</returns>
    Task<AppSettings> LoadAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// 設定を非同期で保存します
    /// </summary>
    /// <param name="filePath">設定ファイルのパス</param>
    /// <param name="settings">保存する設定</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>完了時の Task</returns>
    Task SaveAsync(string filePath, AppSettings settings, CancellationToken cancellationToken = default);
}
