using JsonEditor.App.Infrastructure;
using JsonEditor.App.ViewModels;
using JsonEditor.Core.Models;
using JsonEditor.Core.Services;

namespace JsonEditor.App.Tests;

public class IntegrationTests
{
    [Fact]
    public void OpenValidateAndTreeBuild_WorksEndToEnd()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"json-editor-int-{Guid.NewGuid():N}.json");

        try
        {
            File.WriteAllText(filePath, "{\"name\":\"json\",\"items\":[1,2]}");

            var fileDialog = new StubFileDialogService { OpenPath = filePath };
            var vm = CreateViewModel(fileDialog, new StubMessageBoxService(), new InMemorySettingsStore());

            vm.OpenFileCommand.Execute(null);

            Assert.Equal("File opened", vm.StatusMessage);
            Assert.Equal(filePath, vm.CurrentFilePath);
            Assert.Equal("{\"name\":\"json\",\"items\":[1,2]}", vm.JsonText);
            Assert.Single(vm.TreeNodes);
        }
        finally
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }

    [Fact]
    public void SearchReplaceAndSave_WorksEndToEnd()
    {
        var savePath = Path.Combine(Path.GetTempPath(), $"json-editor-int-{Guid.NewGuid():N}.json");

        try
        {
            var fileDialog = new StubFileDialogService { SavePath = savePath };
            var vm = CreateViewModel(fileDialog, new StubMessageBoxService(), new InMemorySettingsStore());
            vm.JsonText = "{\"name\":\"Json\",\"note\":\"JSON\"}";
            vm.SearchText = "json";
            vm.ReplaceText = "data";

            vm.ReplaceAllCommand.Execute(null);
            vm.SaveAsFileCommand.Execute(null);

            Assert.Contains("data", vm.JsonText, StringComparison.OrdinalIgnoreCase);
            Assert.Equal("File saved", vm.StatusMessage);
            Assert.Equal(vm.JsonText, File.ReadAllText(savePath));
        }
        finally
        {
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }
        }
    }

    [Fact]
    public void BackupRestoreFlow_WorksEndToEnd()
    {
        var savePath = Path.Combine(Path.GetTempPath(), $"json-editor-int-{Guid.NewGuid():N}.json");
        var backupPath = savePath + ".autosave";

        try
        {
            var fileDialog = new StubFileDialogService { SavePath = savePath };
            var vm = CreateViewModel(fileDialog, new StubMessageBoxService(), new InMemorySettingsStore());
            vm.JsonText = "{\"v\":1}";

            vm.SaveAsFileCommand.Execute(null);
            File.WriteAllText(backupPath, "{\"v\":2}");

            vm.RestoreBackupCommand.Execute(null);

            Assert.Equal("{\"v\":2}", vm.JsonText);
            Assert.Equal("Backup restored", vm.StatusMessage);
        }
        finally
        {
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }

            if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
            }
        }
    }

    [Fact]
    public async Task ThemeSettings_ArePersistedAcrossInitializeAndShutdown()
    {
        var store = new InMemorySettingsStore
        {
            Current = new AppSettings
            {
                Theme = "Dark",
                AutoSaveIntervalSeconds = 20,
                UseRegexSearch = true,
                MatchCaseSearch = false
            }
        };

        var vm = CreateViewModel(new StubFileDialogService(), new StubMessageBoxService(), store);

        await vm.InitializeAsync();
        Assert.True(vm.IsDarkTheme);
        Assert.True(vm.IsRegexSearch);

        vm.MatchCaseSearch = true;
        vm.AutoSaveIntervalSeconds = 25;

        await vm.ShutdownAsync();

        Assert.Equal("Dark", store.Current.Theme);
        Assert.Equal(25, store.Current.AutoSaveIntervalSeconds);
        Assert.True(store.Current.UseRegexSearch);
        Assert.True(store.Current.MatchCaseSearch);
    }

    [Fact]
    public void InvalidJson_ShowsValidationError()
    {
        var vm = CreateViewModel(new StubFileDialogService(), new StubMessageBoxService(), new InMemorySettingsStore());
        vm.JsonText = "{";

        vm.ValidateCommand.Execute(null);

        Assert.NotEqual("Valid JSON", vm.StatusMessage);
        Assert.Empty(vm.TreeNodes);
    }

    private static MainViewModel CreateViewModel(IFileDialogService fileDialogService, IMessageBoxService messageBoxService, IAppSettingsStore settingsStore)
    {
        return new MainViewModel(
            new JsonValidationService(),
            new JsonTreeBuilder(),
            new SearchReplaceService(),
            settingsStore,
            fileDialogService,
            messageBoxService,
            false);
    }

    private sealed class StubFileDialogService : IFileDialogService
    {
        public string? OpenPath { get; set; }

        public string? SavePath { get; set; }

        public string? ShowOpenDialog() => OpenPath;

        public string? ShowSaveDialog(string? defaultFileName = null) => SavePath;
    }

    private sealed class StubMessageBoxService : IMessageBoxService
    {
        public int YesNoCancelResult { get; set; }

        public bool ShowConfirm(string message, string title) => true;

        public int ShowYesNoCancel(string message, string title) => YesNoCancelResult;

        public void ShowInfo(string message, string title)
        {
        }
    }

    private sealed class InMemorySettingsStore : IAppSettingsStore
    {
        public AppSettings Current { get; set; } = new();

        public Task<AppSettings> LoadAsync(string filePath, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new AppSettings
            {
                Theme = Current.Theme,
                AutoSaveIntervalSeconds = Current.AutoSaveIntervalSeconds,
                UseRegexSearch = Current.UseRegexSearch,
                MatchCaseSearch = Current.MatchCaseSearch
            });
        }

        public Task SaveAsync(string filePath, AppSettings settings, CancellationToken cancellationToken = default)
        {
            Current = new AppSettings
            {
                Theme = settings.Theme,
                AutoSaveIntervalSeconds = settings.AutoSaveIntervalSeconds,
                UseRegexSearch = settings.UseRegexSearch,
                MatchCaseSearch = settings.MatchCaseSearch
            };

            return Task.CompletedTask;
        }
    }
}
