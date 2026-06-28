using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using JsonEditor.App.ViewModels;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;

namespace JsonEditor.App;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private bool _isEditorUpdateInProgress;
    private const int DwmUseImmersiveDarkModeAttribute = 20;
    private const int DwmUseImmersiveDarkModeLegacyAttribute = 19;

    private const string JsonXshdLight = """
<?xml version="1.0"?>
<SyntaxDefinition name="JSON" extensions=".json" xmlns="http://icsharpcode.net/sharpdevelop/syntaxdefinition/2008">
  <Color name="Default" foreground="#111827" />
  <Color name="String" foreground="#B42318" />
  <Color name="Number" foreground="#1D4ED8" />
  <Color name="Boolean" foreground="#B45309" />
  <Color name="Null" foreground="#6B7280" />
  <Color name="Punctuation" foreground="#334155" />
  <Color name="Property" foreground="#0F766E" />
  <RuleSet>
        <Span color="String" begin="&quot;" end="&quot;" multiline="false" />
        <Keywords color="Boolean">
            <Word>true</Word>
            <Word>false</Word>
        </Keywords>
        <Keywords color="Null">
            <Word>null</Word>
        </Keywords>
    <Rule color="Number">-?\b\d+(\.\d+)?([eE][+-]?\d+)?\b</Rule>
    <Rule color="Punctuation">[\{\}\[\]:,]</Rule>
  </RuleSet>
</SyntaxDefinition>
""";

    private const string JsonXshdDark = """
<?xml version="1.0"?>
<SyntaxDefinition name="JSON" extensions=".json" xmlns="http://icsharpcode.net/sharpdevelop/syntaxdefinition/2008">
  <Color name="Default" foreground="#E5E7EB" />
  <Color name="String" foreground="#FCA5A5" />
  <Color name="Number" foreground="#93C5FD" />
  <Color name="Boolean" foreground="#FCD34D" />
  <Color name="Null" foreground="#94A3B8" />
  <Color name="Punctuation" foreground="#CBD5E1" />
  <Color name="Property" foreground="#5EEAD4" />
  <RuleSet>
        <Span color="String" begin="&quot;" end="&quot;" multiline="false" />
        <Keywords color="Boolean">
            <Word>true</Word>
            <Word>false</Word>
        </Keywords>
        <Keywords color="Null">
            <Word>null</Word>
        </Keywords>
    <Rule color="Number">-?\b\d+(\.\d+)?([eE][+-]?\d+)?\b</Rule>
    <Rule color="Punctuation">[\{\}\[\]:,]</Rule>
  </RuleSet>
</SyntaxDefinition>
""";

    public MainWindow()
    {
        InitializeComponent();
        ApplyEditorHighlighting(false);
    }

    public void ApplyTheme(bool isDarkTheme)
    {
        var source = isDarkTheme
            ? "pack://application:,,,/JsonEditor.App;component/Themes/DarkTheme.xaml"
            : "pack://application:,,,/JsonEditor.App;component/Themes/LightTheme.xaml";
        var dictionaries = Application.Current.Resources.MergedDictionaries;
        dictionaries.Clear();
        dictionaries.Add(new ResourceDictionary { Source = new Uri(source, UriKind.Absolute) });
        ApplyEditorHighlighting(isDarkTheme);
        TryApplyTitleBarTheme(isDarkTheme);
    }

    private void Editor_TextChanged(object? sender, EventArgs e)
    {
        if (_isEditorUpdateInProgress || DataContext is not MainViewModel vm)
        {
            return;
        }

        vm.JsonText = Editor.Text;
    }

    private void ThemeCheckChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            ApplyTheme(vm.IsDarkTheme);
        }
    }

    private void ApplyEditorHighlighting(bool isDarkTheme)
    {
        var xshd = isDarkTheme ? JsonXshdDark : JsonXshdLight;
        using var stringReader = new StringReader(xshd);
        using var xmlReader = XmlReader.Create(stringReader);
        Editor.SyntaxHighlighting = HighlightingLoader.Load(xmlReader, HighlightingManager.Instance);
    }

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        if (DataContext is not MainViewModel vm)
        {
            return;
        }

        vm.PropertyChanged += ViewModelPropertyChanged;
        RefreshEditorFromViewModel(vm);
    }

    private void ViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is not MainViewModel vm)
        {
            return;
        }

        if (e.PropertyName == nameof(MainViewModel.IsDarkTheme))
        {
            ApplyTheme(vm.IsDarkTheme);
            return;
        }

        if (e.PropertyName == nameof(MainViewModel.JsonText))
        {
            RefreshEditorFromViewModel(vm);
            return;
        }

        if (e.PropertyName == nameof(MainViewModel.SelectedMatchStart) || e.PropertyName == nameof(MainViewModel.SelectedMatchLength))
        {
            ApplySelectionFromViewModel(vm);
        }
    }

    private void RefreshEditorFromViewModel(MainViewModel vm)
    {
        if (Editor.Text == vm.JsonText)
        {
            return;
        }

        _isEditorUpdateInProgress = true;
        Editor.Text = vm.JsonText;
        _isEditorUpdateInProgress = false;
    }

    private void ApplySelectionFromViewModel(MainViewModel vm)
    {
        if (vm.SelectedMatchStart < 0 || vm.SelectedMatchStart >= Editor.Text.Length)
        {
            return;
        }

        var length = vm.SelectedMatchLength;
        if (length <= 0)
        {
            return;
        }

        Editor.Select(vm.SelectedMatchStart, length);
        Editor.TextArea.Caret.Offset = vm.SelectedMatchStart + length;
        var location = Editor.Document.GetLocation(vm.SelectedMatchStart);
        Editor.ScrollTo(location.Line, location.Column);
        Editor.Focus();
    }

    private void TryApplyTitleBarTheme(bool isDarkTheme)
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        if (hwnd == IntPtr.Zero)
        {
            return;
        }

        var useDarkMode = isDarkTheme ? 1 : 0;
        var result = DwmSetWindowAttribute(
            hwnd,
            DwmUseImmersiveDarkModeAttribute,
            ref useDarkMode,
            Marshal.SizeOf<int>());

        if (result == 0)
        {
            return;
        }

        _ = DwmSetWindowAttribute(
            hwnd,
            DwmUseImmersiveDarkModeLegacyAttribute,
            ref useDarkMode,
            Marshal.SizeOf<int>());
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int valueSize);
}