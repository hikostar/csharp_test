using System.Reflection;
using System.Threading;
using System.Windows;
using ICSharpCode.AvalonEdit;
using JsonEditor.App;
using JsonEditor.App.Infrastructure;
using JsonEditor.App.ViewModels;
using JsonEditor.Core.Models;
using JsonEditor.Core.Services;

namespace JsonEditor.App.Tests;

public class UIComponentTests
{
    [Fact]
    public void ApplyTheme_UsesDarkThemeDictionary()
    {
        RunInSta(() =>
        {
            EnsureApplication();
            var window = new MainWindow();

            window.ApplyTheme(true);

            Assert.NotEmpty(Application.Current.Resources.MergedDictionaries);
            Assert.Contains("darktheme.xaml", Application.Current.Resources.MergedDictionaries[0].Source.OriginalString, StringComparison.OrdinalIgnoreCase);
        });
    }

    [Fact]
    public void ApplyTheme_UsesLightThemeDictionary()
    {
        RunInSta(() =>
        {
            EnsureApplication();
            var window = new MainWindow();

            window.ApplyTheme(false);

            Assert.NotEmpty(Application.Current.Resources.MergedDictionaries);
            Assert.Contains("lighttheme.xaml", Application.Current.Resources.MergedDictionaries[0].Source.OriginalString, StringComparison.OrdinalIgnoreCase);
        });
    }

    [Fact]
    public void EditorTextChanged_UpdatesViewModelJsonText()
    {
        RunInSta(() =>
        {
            EnsureApplication();
            var window = new MainWindow();
            var vm = CreateViewModel();
            window.DataContext = vm;

            var editor = GetEditor(window);
            editor.Text = "{\"name\":\"editor\"}";

            Assert.Equal("{\"name\":\"editor\"}", vm.JsonText);
        });
    }

    [Fact]
    public void RefreshEditorFromViewModel_ReflectsViewModelText()
    {
        RunInSta(() =>
        {
            EnsureApplication();
            var window = new MainWindow();
            var vm = CreateViewModel();
            window.DataContext = vm;
            vm.JsonText = "{\"source\":\"vm\"}";

            InvokePrivate(window, "RefreshEditorFromViewModel", vm);

            var editor = GetEditor(window);
            Assert.Equal(vm.JsonText, editor.Text);
        });
    }

    [Fact]
    public void ApplySelectionFromViewModel_SelectsMatchedRange()
    {
        RunInSta(() =>
        {
            EnsureApplication();
            var window = new MainWindow();
            var vm = CreateViewModel();
            window.DataContext = vm;

            var editor = GetEditor(window);
            editor.Text = "{\"name\":\"json\"}";
            vm.SearchText = "json";
            vm.NextMatchCommand.Execute(null);

            InvokePrivate(window, "ApplySelectionFromViewModel", vm);

            Assert.Equal(vm.SelectedMatchLength, editor.SelectionLength);
            Assert.Equal(vm.SelectedMatchStart, editor.SelectionStart);
        });
    }

    [Fact]
    public void ThemeCheckChanged_AppliesThemeFromViewModelState()
    {
        RunInSta(() =>
        {
            EnsureApplication();
            var window = new MainWindow();
            var vm = CreateViewModel();
            vm.IsDarkTheme = true;
            window.DataContext = vm;

            InvokePrivate(window, "ThemeCheckChanged", window, new RoutedEventArgs());

            Assert.NotEmpty(Application.Current.Resources.MergedDictionaries);
            Assert.Contains("darktheme.xaml", Application.Current.Resources.MergedDictionaries[0].Source.OriginalString, StringComparison.OrdinalIgnoreCase);
        });
    }

    private static void EnsureApplication()
    {
        if (Application.Current is null)
        {
            _ = new Application();
        }
    }

    private static void RunInSta(Action action)
    {
        Exception? caught = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                caught = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (caught is not null)
        {
            throw new InvalidOperationException("STA test execution failed", caught);
        }
    }

    private static TextEditor GetEditor(MainWindow window)
    {
        return (TextEditor)window.FindName("Editor")!;
    }

    private static void InvokePrivate(object instance, string methodName, params object[] args)
    {
        var method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method);
        method!.Invoke(instance, args);
    }

    private static MainViewModel CreateViewModel()
    {
        return new MainViewModel(
            new JsonValidationService(),
            new JsonTreeBuilder(),
            new SearchReplaceService(),
            new InMemorySettingsStore(),
            new StubFileDialogService(),
            new StubMessageBoxService(),
            false);
    }

    private sealed class StubFileDialogService : IFileDialogService
    {
        public string? ShowOpenDialog() => null;

        public string? ShowSaveDialog(string? defaultFileName = null) => null;
    }

    private sealed class StubMessageBoxService : IMessageBoxService
    {
        public bool ShowConfirm(string message, string title) => true;

        public int ShowYesNoCancel(string message, string title) => 0;

        public void ShowInfo(string message, string title)
        {
        }
    }

    private sealed class InMemorySettingsStore : IAppSettingsStore
    {
        public Task<AppSettings> LoadAsync(string filePath, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new AppSettings());
        }

        public Task SaveAsync(string filePath, AppSettings settings, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
