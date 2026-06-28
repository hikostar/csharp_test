using JsonEditor.Core.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace JsonEditor_WinUI3;

/// <summary>
/// The main content page displayed inside the application window.
/// Add your UI logic, event handlers, and data binding here.
/// </summary>
public sealed partial class MainPage : Page
{
    private readonly JsonValidationService _jsonValidationService = new();

    public MainPage()
    {
        InitializeComponent();
    }

    private async void OpenButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".json");
            picker.FileTypeFilter.Add(".txt");
            InitializePicker(picker);

            var file = await picker.PickSingleFileAsync();
            if (file is null)
            {
                UpdateStatus("Open canceled");
                return;
            }

            EditorTextBox.Text = await FileIO.ReadTextAsync(file);
            UpdateStatus($"Opened: {file.Name}");
        }
        catch (Exception ex)
        {
            UpdateStatus($"Open failed: {ex.Message}");
        }
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var picker = new FileSavePicker
            {
                SuggestedFileName = "document.json"
            };
            picker.FileTypeChoices.Add("JSON File", new List<string> { ".json" });
            picker.FileTypeChoices.Add("Text File", new List<string> { ".txt" });
            InitializePicker(picker);

            var file = await picker.PickSaveFileAsync();
            if (file is null)
            {
                UpdateStatus("Save canceled");
                return;
            }

            await FileIO.WriteTextAsync(file, EditorTextBox.Text ?? string.Empty);
            UpdateStatus($"Saved: {file.Name}");
        }
        catch (Exception ex)
        {
            UpdateStatus($"Save failed: {ex.Message}");
        }
    }

    private void ValidateButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var result = _jsonValidationService.Validate(EditorTextBox.Text ?? string.Empty);
            if (result.IsValid)
            {
                UpdateStatus("JSON is valid");
                return;
            }

            var location = string.Empty;
            if (result.LineNumber is not null && result.BytePositionInLine is not null)
            {
                location = $" (line {result.LineNumber}, position {result.BytePositionInLine})";
            }

            UpdateStatus($"Invalid JSON{location}: {result.ErrorMessage}");
        }
        catch (Exception ex)
        {
            UpdateStatus($"Validation failed: {ex.Message}");
        }
    }

    private static void InitializePicker(object picker)
    {
        var window = App.MainAppWindow;
        if (window is null)
        {
            throw new InvalidOperationException("Main window is not initialized.");
        }

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
    }

    private void UpdateStatus(string message)
    {
        StatusTextBlock.Text = message;
    }
}
