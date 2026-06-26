using Moq;
using JsonEditor.App.Infrastructure;
using JsonEditor.App.ViewModels;
using JsonEditor.Core.Models;
using JsonEditor.Core.Services;

namespace JsonEditor.App.Tests;

public class MainViewModelTests
{
    [Fact]
    public void Constructor_Throws_WhenValidationServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new MainViewModel(
                null!,
                Mock.Of<IJsonTreeBuilder>(),
                Mock.Of<ISearchReplaceService>(),
                Mock.Of<IAppSettingsStore>(),
                Mock.Of<IFileDialogService>(),
                Mock.Of<IMessageBoxService>(),
                false));
    }

    [Fact]
    public void ValidateCommand_SetsStatusValid_AndBuildsTree()
    {
        var fixture = CreateFixture();
        var rootNode = new JsonTreeNode { Label = "root" };
        rootNode.Children.Add(new JsonTreeNode { Label = "child" });

        fixture.ValidationService.Setup(x => x.Validate(It.IsAny<string>())).Returns(JsonValidationResult.Valid());
        fixture.TreeBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns(rootNode);

        fixture.ViewModel.JsonText = "{\"x\":1}";
        fixture.ViewModel.ValidateCommand.Execute(null);

        Assert.Equal("Valid JSON", fixture.ViewModel.StatusMessage);
        Assert.Single(fixture.ViewModel.TreeNodes);
        Assert.Equal("root", fixture.ViewModel.TreeNodes[0].Label);
        Assert.Single(fixture.ViewModel.TreeNodes[0].Children);
    }

    [Fact]
    public void ValidateCommand_SetsErrorStatus_WhenJsonIsInvalid()
    {
        var fixture = CreateFixture();
        fixture.ValidationService.Setup(x => x.Validate(It.IsAny<string>())).Returns(JsonValidationResult.Invalid("broken"));

        fixture.ViewModel.JsonText = "{";
        fixture.ViewModel.ValidateCommand.Execute(null);

        Assert.Equal("broken", fixture.ViewModel.StatusMessage);
        Assert.Empty(fixture.ViewModel.TreeNodes);
    }

    [Fact]
    public void ReplaceAllCommand_ReplacesJsonText_AndSetsCompletedStatus()
    {
        var fixture = CreateFixture();
        fixture.ValidationService.Setup(x => x.Validate(It.IsAny<string>())).Returns(JsonValidationResult.Valid());
        fixture.TreeBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns((JsonTreeNode?)null);
        fixture.SearchReplaceService.Setup(x => x.ReplaceAll(It.IsAny<string>(), It.IsAny<SearchOptions>())).Returns("result");
        fixture.SearchReplaceService.Setup(x => x.CountMatches(It.IsAny<string>(), It.IsAny<SearchOptions>())).Returns(1);

        fixture.ViewModel.JsonText = "before";
        fixture.ViewModel.SearchText = "be";
        fixture.ViewModel.ReplaceText = "re";

        fixture.ViewModel.ReplaceAllCommand.Execute(null);

        Assert.Equal("result", fixture.ViewModel.JsonText);
        Assert.Equal("Replace completed", fixture.ViewModel.StatusMessage);
    }

    [Fact]
    public void ReplaceAllCommand_SetsErrorStatus_WhenServiceThrows()
    {
        var fixture = CreateFixture();
        fixture.ValidationService.Setup(x => x.Validate(It.IsAny<string>())).Returns(JsonValidationResult.Valid());
        fixture.TreeBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns((JsonTreeNode?)null);
        fixture.SearchReplaceService
            .Setup(x => x.ReplaceAll(It.IsAny<string>(), It.IsAny<SearchOptions>()))
            .Throws(new InvalidOperationException("replace failed"));

        fixture.ViewModel.JsonText = "before";
        fixture.ViewModel.SearchText = "be";

        fixture.ViewModel.ReplaceAllCommand.Execute(null);

        Assert.Equal("Replace error: replace failed", fixture.ViewModel.StatusMessage);
    }

    [Fact]
    public void NextMatchCommand_UpdatesSelection_WhenMatchExists()
    {
        var fixture = CreateFixture();
        fixture.SearchReplaceService
            .Setup(x => x.FindNextMatch(It.IsAny<string>(), It.IsAny<SearchOptions>(), It.IsAny<int>()))
            .Returns((5, 3));

        fixture.ViewModel.JsonText = "abc123abc";
        fixture.ViewModel.SearchText = "123";

        fixture.ViewModel.NextMatchCommand.Execute(null);

        Assert.Equal(5, fixture.ViewModel.SelectedMatchStart);
        Assert.Equal(3, fixture.ViewModel.SelectedMatchLength);
        Assert.Equal("Jumped to match at 5", fixture.ViewModel.StatusMessage);
    }

    [Fact]
    public void PreviousMatchCommand_UpdatesSelection_WhenMatchExists()
    {
        var fixture = CreateFixture();
        fixture.SearchReplaceService
            .Setup(x => x.FindPreviousMatch(It.IsAny<string>(), It.IsAny<SearchOptions>(), It.IsAny<int>()))
            .Returns((2, 2));

        fixture.ViewModel.JsonText = "abABab";
        fixture.ViewModel.SearchText = "AB";

        fixture.ViewModel.PreviousMatchCommand.Execute(null);

        Assert.Equal(2, fixture.ViewModel.SelectedMatchStart);
        Assert.Equal(2, fixture.ViewModel.SelectedMatchLength);
        Assert.Equal("Jumped to match at 2", fixture.ViewModel.StatusMessage);
    }

    [Fact]
    public void BuildReplacePreviewCommand_GeneratesPreviewItems()
    {
        var fixture = CreateFixture();
        var previews = new List<ReplacePreviewItem>
        {
            new() { Start = 0, Length = 3, OriginalText = "foo", ReplacementText = "bar" },
            new() { Start = 8, Length = 3, OriginalText = "foo", ReplacementText = "bar" }
        };
        var totalMatches = 2;

        fixture.SearchReplaceService
            .Setup(x => x.BuildReplacePreview(It.IsAny<string>(), It.IsAny<SearchOptions>(), 30, out totalMatches))
            .Returns(previews);

        fixture.ViewModel.JsonText = "foo and foo";
        fixture.ViewModel.SearchText = "foo";
        fixture.ViewModel.ReplaceText = "bar";

        fixture.ViewModel.BuildReplacePreviewCommand.Execute(null);

        Assert.Equal(2, fixture.ViewModel.ReplacePreviewItems.Count);
        Assert.Equal("Preview: 2 match(es)", fixture.ViewModel.ReplacePreviewSummary);
        Assert.Equal("Replace preview generated", fixture.ViewModel.StatusMessage);
    }

    [Fact]
    public void BuildReplacePreviewCommand_SetsErrorStatus_WhenRegexIsInvalid()
    {
        var validationService = new Mock<IJsonValidationService>();
        validationService.Setup(x => x.Validate(It.IsAny<string>())).Returns(JsonValidationResult.Valid());

        var viewModel = new MainViewModel(
            validationService.Object,
            Mock.Of<IJsonTreeBuilder>(),
            new SearchReplaceService(),
            Mock.Of<IAppSettingsStore>(),
            Mock.Of<IFileDialogService>(),
            Mock.Of<IMessageBoxService>(),
            false);

        viewModel.JsonText = "abc";
        viewModel.IsRegexSearch = true;
        viewModel.SearchText = "(";

        viewModel.BuildReplacePreviewCommand.Execute(null);

        Assert.Equal("Preview failed", viewModel.ReplacePreviewSummary);
        Assert.StartsWith("Preview error:", viewModel.StatusMessage);
    }

    [Fact]
    public void ToggleThemeCommand_TogglesThemeFlag()
    {
        var fixture = CreateFixture();

        Assert.False(fixture.ViewModel.IsDarkTheme);

        fixture.ViewModel.ToggleThemeCommand.Execute(null);

        Assert.True(fixture.ViewModel.IsDarkTheme);
    }

    [Fact]
    public void OpenFileCommand_LoadsSelectedFile()
    {
        var fixture = CreateFixture();
        fixture.ValidationService.Setup(x => x.Validate(It.IsAny<string>())).Returns(JsonValidationResult.Valid());
        fixture.TreeBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns((JsonTreeNode?)null);

        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, "{\"name\":\"value\"}");
            fixture.FileDialogService.Setup(x => x.ShowOpenDialog()).Returns(path);

            fixture.ViewModel.OpenFileCommand.Execute(null);

            Assert.Equal(path, fixture.ViewModel.CurrentFilePath);
            Assert.Equal("{\"name\":\"value\"}", fixture.ViewModel.JsonText);
            Assert.Equal("File opened", fixture.ViewModel.StatusMessage);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public void OpenFileCommand_RestoresBackup_WhenUserChoosesYes()
    {
        var fixture = CreateFixture();
        fixture.ValidationService.Setup(x => x.Validate(It.IsAny<string>())).Returns(JsonValidationResult.Valid());
        fixture.TreeBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns((JsonTreeNode?)null);

        var filePath = Path.GetTempFileName();
        var backupPath = filePath + ".autosave";

        try
        {
            File.WriteAllText(filePath, "{\"version\":1}");
            File.WriteAllText(backupPath, "{\"version\":2}");
            var baseTime = DateTime.UtcNow.AddHours(-1);
            File.SetLastWriteTimeUtc(filePath, baseTime);
            File.SetLastWriteTimeUtc(backupPath, baseTime.AddMinutes(30));

            fixture.FileDialogService.Setup(x => x.ShowOpenDialog()).Returns(filePath);
            fixture.MessageBoxService.Setup(x => x.ShowYesNoCancel(It.IsAny<string>(), "Restore Backup")).Returns(1);

            fixture.ViewModel.OpenFileCommand.Execute(null);

            Assert.Equal("{\"version\":2}", fixture.ViewModel.JsonText);
            Assert.Equal("Backup restored while opening file", fixture.ViewModel.StatusMessage);
        }
        finally
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
            }
        }
    }

    [Fact]
    public void OpenFileCommand_Cancels_WhenUserChoosesCancelForBackupRestore()
    {
        var fixture = CreateFixture();
        fixture.ValidationService.Setup(x => x.Validate(It.IsAny<string>())).Returns(JsonValidationResult.Valid());
        fixture.TreeBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns((JsonTreeNode?)null);

        var filePath = Path.GetTempFileName();
        var backupPath = filePath + ".autosave";

        try
        {
            File.WriteAllText(filePath, "{\"value\":1}");
            File.WriteAllText(backupPath, "{\"value\":2}");
            var baseTime = DateTime.UtcNow.AddHours(-1);
            File.SetLastWriteTimeUtc(filePath, baseTime);
            File.SetLastWriteTimeUtc(backupPath, baseTime.AddMinutes(30));

            fixture.FileDialogService.Setup(x => x.ShowOpenDialog()).Returns(filePath);
            fixture.MessageBoxService.Setup(x => x.ShowYesNoCancel(It.IsAny<string>(), "Restore Backup")).Returns(-1);

            fixture.ViewModel.OpenFileCommand.Execute(null);

            Assert.Equal(string.Empty, fixture.ViewModel.CurrentFilePath);
            Assert.Equal(string.Empty, fixture.ViewModel.JsonText);
        }
        finally
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
            }
        }
    }

    [Fact]
    public void SaveAsFileCommand_SavesJsonToSelectedPath()
    {
        var fixture = CreateFixture();
        fixture.ValidationService.Setup(x => x.Validate(It.IsAny<string>())).Returns(JsonValidationResult.Valid());
        fixture.TreeBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns((JsonTreeNode?)null);

        var path = Path.Combine(Path.GetTempPath(), $"json-editor-{Guid.NewGuid():N}.json");
        try
        {
            fixture.FileDialogService.Setup(x => x.ShowSaveDialog(It.IsAny<string?>())).Returns(path);
            fixture.ViewModel.JsonText = "{\"saved\":true}";

            fixture.ViewModel.SaveAsFileCommand.Execute(null);

            Assert.True(File.Exists(path));
            Assert.Equal("{\"saved\":true}", File.ReadAllText(path));
            Assert.Equal(path, fixture.ViewModel.CurrentFilePath);
            Assert.Equal("File saved", fixture.ViewModel.StatusMessage);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public void RestoreBackupCommand_LoadsBackupForCurrentFile()
    {
        var fixture = CreateFixture();
        fixture.ValidationService.Setup(x => x.Validate(It.IsAny<string>())).Returns(JsonValidationResult.Valid());
        fixture.TreeBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns((JsonTreeNode?)null);

        var path = Path.Combine(Path.GetTempPath(), $"json-editor-{Guid.NewGuid():N}.json");
        var backupPath = path + ".autosave";

        try
        {
            fixture.FileDialogService.Setup(x => x.ShowSaveDialog(It.IsAny<string?>())).Returns(path);
            fixture.ViewModel.JsonText = "{\"value\":1}";
            fixture.ViewModel.SaveAsFileCommand.Execute(null);

            File.WriteAllText(backupPath, "{\"value\":2}");

            fixture.ViewModel.RestoreBackupCommand.Execute(null);

            Assert.Equal("{\"value\":2}", fixture.ViewModel.JsonText);
            Assert.Equal("Backup restored", fixture.ViewModel.StatusMessage);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
            }
        }
    }

    [Fact]
    public async Task InitializeAsync_LoadsPersistedSettings()
    {
        var fixture = CreateFixture();
        fixture.SettingsStore
            .Setup(x => x.LoadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AppSettings
            {
                Theme = "Dark",
                AutoSaveIntervalSeconds = 42,
                UseRegexSearch = true,
                MatchCaseSearch = true
            });

        await fixture.ViewModel.InitializeAsync();

        Assert.True(fixture.ViewModel.IsDarkTheme);
        Assert.Equal(42, fixture.ViewModel.AutoSaveIntervalSeconds);
        Assert.True(fixture.ViewModel.IsRegexSearch);
        Assert.True(fixture.ViewModel.MatchCaseSearch);
    }

    [Fact]
    public async Task ShutdownAsync_PersistsCurrentSettings()
    {
        var fixture = CreateFixture();
        fixture.ViewModel.IsDarkTheme = true;
        fixture.ViewModel.AutoSaveIntervalSeconds = 12;
        fixture.ViewModel.IsRegexSearch = true;
        fixture.ViewModel.MatchCaseSearch = true;

        await fixture.ViewModel.ShutdownAsync();

        fixture.SettingsStore.Verify(
            x => x.SaveAsync(
                It.IsAny<string>(),
                It.Is<AppSettings>(s =>
                    s.Theme == "Dark" &&
                    s.AutoSaveIntervalSeconds == 12 &&
                    s.UseRegexSearch &&
                    s.MatchCaseSearch),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static MainViewModelFixture CreateFixture()
    {
        var validation = new Mock<IJsonValidationService>();
        var treeBuilder = new Mock<IJsonTreeBuilder>();
        var searchReplace = new Mock<ISearchReplaceService>();
        var settings = new Mock<IAppSettingsStore>();
        var fileDialog = new Mock<IFileDialogService>();
        var messageBox = new Mock<IMessageBoxService>();

        validation.Setup(x => x.Validate(It.IsAny<string>())).Returns(JsonValidationResult.Valid());
        treeBuilder.Setup(x => x.Build(It.IsAny<string>())).Returns((JsonTreeNode?)null);
        searchReplace.Setup(x => x.CountMatches(It.IsAny<string>(), It.IsAny<SearchOptions>())).Returns(0);
        settings.Setup(x => x.LoadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new AppSettings());
        settings.Setup(x => x.SaveAsync(It.IsAny<string>(), It.IsAny<AppSettings>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var viewModel = new MainViewModel(
            validation.Object,
            treeBuilder.Object,
            searchReplace.Object,
            settings.Object,
            fileDialog.Object,
            messageBox.Object,
            false);

        return new MainViewModelFixture(viewModel, validation, treeBuilder, searchReplace, settings, fileDialog, messageBox);
    }

    private sealed record MainViewModelFixture(
        MainViewModel ViewModel,
        Mock<IJsonValidationService> ValidationService,
        Mock<IJsonTreeBuilder> TreeBuilder,
        Mock<ISearchReplaceService> SearchReplaceService,
        Mock<IAppSettingsStore> SettingsStore,
        Mock<IFileDialogService> FileDialogService,
        Mock<IMessageBoxService> MessageBoxService);
}
