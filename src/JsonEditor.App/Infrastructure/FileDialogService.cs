using Microsoft.Win32;

namespace JsonEditor.App.Infrastructure;

/// <summary>
/// WPF のネイティブダイアログを使用したファイルダイアログ実装
/// </summary>
public sealed class FileDialogService : IFileDialogService
{
    private const string JsonFilter = "JSON files (*.json)|*.json|All files (*.*)|*.*";

    public string? ShowOpenDialog()
    {
        var dialog = new OpenFileDialog
        {
            Filter = JsonFilter,
            CheckFileExists = true,
            CheckPathExists = true
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? ShowSaveDialog(string? defaultFileName = null)
    {
        var dialog = new SaveFileDialog
        {
            Filter = JsonFilter,
            FileName = defaultFileName ?? "document.json",
            AddExtension = true,
            DefaultExt = ".json"
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
