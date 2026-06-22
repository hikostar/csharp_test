using System.Windows;
using System.Windows.Controls;
using JsonEditor.App.ViewModels;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System.IO;
using System.Xml;

namespace JsonEditor.App;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private bool _isEditorUpdateInProgress;

    public MainWindow()
    {
        InitializeComponent();
        ConfigureHighlighting();
    }

    public void ApplyTheme(bool isDarkTheme)
    {
        var source = isDarkTheme ? "Themes/DarkTheme.xaml" : "Themes/LightTheme.xaml";
        var dictionaries = Application.Current.Resources.MergedDictionaries;
        dictionaries.Clear();
        dictionaries.Add(new ResourceDictionary { Source = new Uri(source, UriKind.Relative) });
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

    private void ConfigureHighlighting()
    {
        const string jsonXshd = """
<?xml version="1.0"?>
<SyntaxDefinition name="JSON" extensions=".json" xmlns="http://icsharpcode.net/sharpdevelop/syntaxdefinition/2008">
  <Color name="Default" foreground="Black" />
  <Color name="String" foreground="#B03060" />
  <Color name="Number" foreground="#1E40AF" />
  <Color name="Boolean" foreground="#B45309" />
  <Color name="Null" foreground="#6B7280" />
  <Color name="Punctuation" foreground="#374151" />
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

        using var stringReader = new StringReader(jsonXshd);
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
}