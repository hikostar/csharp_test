using System.Windows;

namespace JsonEditor.App.Infrastructure;

/// <summary>
/// WPF の MessageBox を使用したメッセージボックス実装
/// </summary>
public sealed class MessageBoxService : IMessageBoxService
{
    public bool ShowConfirm(string message, string title)
    {
        var result = MessageBox.Show(
            message,
            title,
            MessageBoxButton.OKCancel,
            MessageBoxImage.Question);

        return result == MessageBoxResult.OK;
    }

    public int ShowYesNoCancel(string message, string title)
    {
        var result = MessageBox.Show(
            message,
            title,
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Question);

        return result switch
        {
            MessageBoxResult.Yes => 1,
            MessageBoxResult.No => 0,
            MessageBoxResult.Cancel => -1,
            _ => -1
        };
    }

    public void ShowInfo(string message, string title)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
