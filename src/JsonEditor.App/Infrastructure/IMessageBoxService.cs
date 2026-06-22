namespace JsonEditor.App.Infrastructure;

/// <summary>
/// メッセージボックスダイアログ操作を抽象化するインターフェース
/// </summary>
public interface IMessageBoxService
{
    /// <summary>
    /// 確認ダイアログを表示します
    /// </summary>
    /// <param name="message">メッセージ</param>
    /// <param name="title">タイトル</param>
    /// <returns>Yes → true, No/Cancel → false</returns>
    bool ShowConfirm(string message, string title);

    /// <summary>
    /// Yes/No/Cancel の選択ダイアログを表示します
    /// </summary>
    /// <param name="message">メッセージ</param>
    /// <param name="title">タイトル</param>
    /// <returns>Yes → 1, No → 0, Cancel → -1</returns>
    int ShowYesNoCancel(string message, string title);

    /// <summary>
    /// 情報メッセージを表示します
    /// </summary>
    /// <param name="message">メッセージ</param>
    /// <param name="title">タイトル</param>
    void ShowInfo(string message, string title);
}
