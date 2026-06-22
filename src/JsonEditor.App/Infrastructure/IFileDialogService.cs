namespace JsonEditor.App.Infrastructure;

/// <summary>
/// ファイルダイアログ操作を抽象化するインターフェース
/// </summary>
public interface IFileDialogService
{
    /// <summary>
    /// ファイルを開くダイアログを表示します
    /// </summary>
    /// <returns>選択されたファイルパス（キャンセル時は null）</returns>
    string? ShowOpenDialog();

    /// <summary>
    /// ファイルを保存するダイアログを表示します
    /// </summary>
    /// <param name="defaultFileName">デフォルトファイル名</param>
    /// <returns>選択されたファイルパス（キャンセル時は null）</returns>
    string? ShowSaveDialog(string? defaultFileName = null);
}
