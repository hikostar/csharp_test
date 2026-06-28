using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using JsonEditor.App.Infrastructure;
using JsonEditor.Core.Models;
using JsonEditor.Core.Services;

namespace JsonEditor.App.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly IJsonValidationService _validationService;
    private readonly IJsonTreeBuilder _treeBuilder;
    private readonly ISearchReplaceService _searchReplaceService;
    private readonly IAppSettingsStore _settingsStore;
    private readonly IFileDialogService _fileDialogService;
    private readonly IMessageBoxService _messageBoxService;
    private readonly bool _autoSaveLoopEnabled;

    private string _jsonText = string.Empty;
    private string _searchText = string.Empty;
    private string _replaceText = string.Empty;
    private string _statusMessage = "Ready";
    private string _currentFilePath = string.Empty;
    private string _replacePreviewSummary = "Preview: not generated";
    private bool _isDarkTheme;
    private bool _isRegexSearch;
    private bool _matchCaseSearch;
    private DateTime _lastEditTimeUtc = DateTime.MinValue;
    private int _autoSaveIntervalSeconds = 30;
    private int _selectedMatchStart = -1;
    private int _selectedMatchLength;

    public MainViewModel()
        : this(
            new JsonValidationService(),
            new JsonTreeBuilder(),
            new SearchReplaceService(),
            new AppSettingsStore(),
            new FileDialogService(),
            new MessageBoxService())
    {
    }

    public MainViewModel(
        IJsonValidationService validationService,
        IJsonTreeBuilder treeBuilder,
        ISearchReplaceService searchReplaceService,
        IAppSettingsStore settingsStore,
        IFileDialogService fileDialogService,
        IMessageBoxService messageBoxService,
        bool startAutoSaveLoop = true)
    {
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _treeBuilder = treeBuilder ?? throw new ArgumentNullException(nameof(treeBuilder));
        _searchReplaceService = searchReplaceService ?? throw new ArgumentNullException(nameof(searchReplaceService));
        _settingsStore = settingsStore ?? throw new ArgumentNullException(nameof(settingsStore));
        _fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
        _messageBoxService = messageBoxService ?? throw new ArgumentNullException(nameof(messageBoxService));
        _autoSaveLoopEnabled = startAutoSaveLoop;

        OpenFileCommand = new RelayCommand(OpenFile);
        SaveFileCommand = new RelayCommand(SaveFile, () => !string.IsNullOrWhiteSpace(CurrentFilePath));
        SaveAsFileCommand = new RelayCommand(SaveAsFile);
        RestoreBackupCommand = new RelayCommand(RestoreBackupForCurrentFile, () => !string.IsNullOrWhiteSpace(CurrentFilePath));
        ValidateCommand = new RelayCommand(ValidateAndRebuildTree);
        ReplaceAllCommand = new RelayCommand(ReplaceAll);
        NextMatchCommand = new RelayCommand(GoToNextMatch);
        PreviousMatchCommand = new RelayCommand(GoToPreviousMatch);
        BuildReplacePreviewCommand = new RelayCommand(BuildReplacePreview);
        ToggleThemeCommand = new RelayCommand(() => IsDarkTheme = !IsDarkTheme);

        AutoSaveTimer = new PeriodicTimer(TimeSpan.FromSeconds(AutoSaveIntervalSeconds));
        if (_autoSaveLoopEnabled)
        {
            _ = RunAutoSaveLoopAsync();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<JsonTreeNodeViewModel> TreeNodes { get; } = new();
    public ObservableCollection<string> ReplacePreviewItems { get; } = new();

    public ICommand OpenFileCommand { get; }
    public ICommand SaveFileCommand { get; }
    public ICommand SaveAsFileCommand { get; }
    public ICommand RestoreBackupCommand { get; }
    public ICommand ValidateCommand { get; }
    public ICommand ReplaceAllCommand { get; }
    public ICommand NextMatchCommand { get; }
    public ICommand PreviousMatchCommand { get; }
    public ICommand BuildReplacePreviewCommand { get; }
    public ICommand ToggleThemeCommand { get; }

    public PeriodicTimer AutoSaveTimer { get; private set; }

    public string JsonText
    {
        get => _jsonText;
        set
        {
            if (_jsonText == value)
            {
                return;
            }

            _jsonText = value;
            _lastEditTimeUtc = DateTime.UtcNow;
            OnPropertyChanged();
            ValidateAndRebuildTree();
        }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText == value)
            {
                return;
            }

            _searchText = value;
            OnPropertyChanged();
            UpdateSearchStatus();
        }
    }

    public string ReplaceText
    {
        get => _replaceText;
        set
        {
            if (_replaceText == value)
            {
                return;
            }

            _replaceText = value;
            OnPropertyChanged();
        }
    }

    public int SelectedMatchStart
    {
        get => _selectedMatchStart;
        private set
        {
            if (_selectedMatchStart == value)
            {
                return;
            }

            _selectedMatchStart = value;
            OnPropertyChanged();
        }
    }

    public int SelectedMatchLength
    {
        get => _selectedMatchLength;
        private set
        {
            if (_selectedMatchLength == value)
            {
                return;
            }

            _selectedMatchLength = value;
            OnPropertyChanged();
        }
    }

    public bool MatchCaseSearch
    {
        get => _matchCaseSearch;
        set
        {
            if (_matchCaseSearch == value)
            {
                return;
            }

            _matchCaseSearch = value;
            OnPropertyChanged();
            UpdateSearchStatus();
        }
    }

    public bool IsRegexSearch
    {
        get => _isRegexSearch;
        set
        {
            if (_isRegexSearch == value)
            {
                return;
            }

            _isRegexSearch = value;
            OnPropertyChanged();
            UpdateSearchStatus();
        }
    }

    public bool IsDarkTheme
    {
        get => _isDarkTheme;
        set
        {
            if (_isDarkTheme == value)
            {
                return;
            }

            _isDarkTheme = value;
            OnPropertyChanged();
        }
    }

    public int AutoSaveIntervalSeconds
    {
        get => _autoSaveIntervalSeconds;
        set
        {
            if (_autoSaveIntervalSeconds == value || value < 5)
            {
                return;
            }

            _autoSaveIntervalSeconds = value;
            AutoSaveTimer.Dispose();
            AutoSaveTimer = new PeriodicTimer(TimeSpan.FromSeconds(AutoSaveIntervalSeconds));
            if (_autoSaveLoopEnabled)
            {
                _ = RunAutoSaveLoopAsync();
            }
            OnPropertyChanged();
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (_statusMessage == value)
            {
                return;
            }

            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public string ReplacePreviewSummary
    {
        get => _replacePreviewSummary;
        private set
        {
            if (_replacePreviewSummary == value)
            {
                return;
            }

            _replacePreviewSummary = value;
            OnPropertyChanged();
        }
    }

    public string CurrentFilePath
    {
        get => _currentFilePath;
        private set
        {
            if (_currentFilePath == value)
            {
                return;
            }

            _currentFilePath = value;
            OnPropertyChanged();
            (SaveFileCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (RestoreBackupCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }

    public async Task InitializeAsync()
    {
        var settingsPath = BuildSettingsPath();
        var settings = await _settingsStore.LoadAsync(settingsPath);
        IsDarkTheme = settings.Theme.Equals("Dark", StringComparison.OrdinalIgnoreCase);
        AutoSaveIntervalSeconds = settings.AutoSaveIntervalSeconds;
        IsRegexSearch = settings.UseRegexSearch;
        MatchCaseSearch = settings.MatchCaseSearch;
    }

    public async Task ShutdownAsync()
    {
        var settings = new AppSettings
        {
            Theme = IsDarkTheme ? "Dark" : "Light",
            AutoSaveIntervalSeconds = AutoSaveIntervalSeconds,
            UseRegexSearch = IsRegexSearch,
            MatchCaseSearch = MatchCaseSearch
        };

        await _settingsStore.SaveAsync(BuildSettingsPath(), settings);
        AutoSaveTimer.Dispose();
    }

    private async Task RunAutoSaveLoopAsync()
    {
        try
        {
            while (await AutoSaveTimer.WaitForNextTickAsync())
            {
                await RunAutoSaveOnceAsync();
            }
        }
        catch (ObjectDisposedException)
        {
        }
    }

    internal async Task RunAutoSaveOnceAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentFilePath))
        {
            return;
        }

        var hasRecentEdit = DateTime.UtcNow - _lastEditTimeUtc < TimeSpan.FromSeconds(AutoSaveIntervalSeconds);
        if (!hasRecentEdit)
        {
            return;
        }

        await TryAutoSaveBackupAsync();
    }

    private async Task TryAutoSaveBackupAsync()
    {
        try
        {
            await File.WriteAllTextAsync(CurrentFilePath + ".autosave", JsonText);
            StatusMessage = "Autosaved backup";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Autosave failed: {ex.Message}";
        }
    }

    private void OpenFile()
    {
        var selectedFilePath = _fileDialogService.ShowOpenDialog();
        if (string.IsNullOrWhiteSpace(selectedFilePath))
        {
            return;
        }

        var restoredFromBackup = false;
        var fileText = File.ReadAllText(selectedFilePath);
        var backupPath = BuildBackupPath(selectedFilePath);
        if (File.Exists(backupPath))
        {
            var fileTime = File.GetLastWriteTimeUtc(selectedFilePath);
            var backupTime = File.GetLastWriteTimeUtc(backupPath);
            if (backupTime > fileTime)
            {
                var choice = _messageBoxService.ShowYesNoCancel(
                    "新しい自動保存バックアップが見つかりました。復元しますか？\nYes: 復元 / No: 元ファイルを開く / Cancel: 開く処理を中止",
                    "Restore Backup");

                if (choice < 0)
                {
                    return;
                }

                if (choice > 0)
                {
                    fileText = File.ReadAllText(backupPath);
                    restoredFromBackup = true;
                }
            }
        }

        JsonText = fileText;
        CurrentFilePath = selectedFilePath;
        if (restoredFromBackup)
        {
            StatusMessage = "Backup restored while opening file";
            return;
        }

        if (StatusMessage != "Backup restored while opening file")
        {
            StatusMessage = "File opened";
        }
    }

    private void SaveFile()
    {
        File.WriteAllText(CurrentFilePath, JsonText);
        StatusMessage = "File saved";
    }

    private void SaveAsFile()
    {
        var defaultFileName = string.IsNullOrWhiteSpace(CurrentFilePath) ? null : Path.GetFileName(CurrentFilePath);
        var selectedFilePath = _fileDialogService.ShowSaveDialog(defaultFileName);
        if (string.IsNullOrWhiteSpace(selectedFilePath))
        {
            return;
        }

        CurrentFilePath = selectedFilePath;
        SaveFile();
    }

    private void ReplaceAll()
    {
        try
        {
            var options = new SearchOptions
            {
                SearchText = SearchText,
                ReplaceText = ReplaceText,
                MatchCase = MatchCaseSearch,
                UseRegex = IsRegexSearch
            };

            JsonText = _searchReplaceService.ReplaceAll(JsonText, options);
            UpdateSearchStatus();
            StatusMessage = "Replace completed";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Replace error: {ex.Message}";
        }
    }

    private void GoToNextMatch()
    {
        try
        {
            var options = new SearchOptions
            {
                SearchText = SearchText,
                MatchCase = MatchCaseSearch,
                UseRegex = IsRegexSearch
            };

            var from = SelectedMatchStart >= 0 ? SelectedMatchStart + Math.Max(1, SelectedMatchLength) : 0;
            var match = _searchReplaceService.FindNextMatch(JsonText, options, from);
            if (match is null)
            {
                StatusMessage = "No match";
                SelectedMatchStart = -1;
                SelectedMatchLength = 0;
                return;
            }

            SelectedMatchStart = match.Value.Start;
            SelectedMatchLength = match.Value.Length;
            StatusMessage = $"Jumped to match at {SelectedMatchStart}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Search error: {ex.Message}";
        }
    }

    private void GoToPreviousMatch()
    {
        try
        {
            var options = new SearchOptions
            {
                SearchText = SearchText,
                MatchCase = MatchCaseSearch,
                UseRegex = IsRegexSearch
            };

            var from = SelectedMatchStart >= 0 ? SelectedMatchStart : JsonText.Length;
            var match = _searchReplaceService.FindPreviousMatch(JsonText, options, from);
            if (match is null)
            {
                StatusMessage = "No match";
                SelectedMatchStart = -1;
                SelectedMatchLength = 0;
                return;
            }

            SelectedMatchStart = match.Value.Start;
            SelectedMatchLength = match.Value.Length;
            StatusMessage = $"Jumped to match at {SelectedMatchStart}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Search error: {ex.Message}";
        }
    }

    private void BuildReplacePreview()
    {
        ReplacePreviewItems.Clear();
        try
        {
            var options = new SearchOptions
            {
                SearchText = SearchText,
                ReplaceText = ReplaceText,
                MatchCase = MatchCaseSearch,
                UseRegex = IsRegexSearch
            };

            var previews = _searchReplaceService.BuildReplacePreview(JsonText, options, 30, out var totalMatches);
            foreach (var preview in previews)
            {
                ReplacePreviewItems.Add($"{preview.Start}: '{preview.OriginalText}' => '{preview.ReplacementText}'");
            }

            var limitedMessage = totalMatches > previews.Count ? $" (showing {previews.Count})" : string.Empty;
            ReplacePreviewSummary = $"Preview: {totalMatches} match(es){limitedMessage}";
            StatusMessage = "Replace preview generated";
        }
        catch (Exception ex)
        {
            ReplacePreviewSummary = "Preview failed";
            StatusMessage = $"Preview error: {ex.Message}";
        }
    }

    private void RestoreBackupForCurrentFile()
    {
        if (string.IsNullOrWhiteSpace(CurrentFilePath))
        {
            return;
        }

        var backupPath = BuildBackupPath(CurrentFilePath);
        if (!File.Exists(backupPath))
        {
            StatusMessage = "No backup found";
            return;
        }

        JsonText = File.ReadAllText(backupPath);
        StatusMessage = "Backup restored";
    }

    private void UpdateSearchStatus()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return;
        }

        try
        {
            var options = new SearchOptions
            {
                SearchText = SearchText,
                MatchCase = MatchCaseSearch,
                UseRegex = IsRegexSearch
            };
            var count = _searchReplaceService.CountMatches(JsonText, options);
            StatusMessage = $"Matches: {count}";
            if (count == 0)
            {
                SelectedMatchStart = -1;
                SelectedMatchLength = 0;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Search error: {ex.Message}";
        }
    }

    private void ValidateAndRebuildTree()
    {
        var validationResult = _validationService.Validate(JsonText);
        if (!validationResult.IsValid)
        {
            StatusMessage = BuildValidationErrorStatus(validationResult);
            return;
        }

        StatusMessage = "Valid JSON";
        var rootNode = _treeBuilder.Build(JsonText);
        if (rootNode is null)
        {
            TreeNodes.Clear();
            return;
        }

        TreeNodes.Clear();
        TreeNodes.Add(ToViewModel(rootNode));
    }

    private static string BuildValidationErrorStatus(JsonValidationResult validationResult)
    {
        var baseMessage = validationResult.ErrorMessage ?? "Invalid JSON";
        if (!validationResult.LineNumber.HasValue || !validationResult.BytePositionInLine.HasValue)
        {
            return baseMessage;
        }

        var line = validationResult.LineNumber.Value + 1;
        var column = validationResult.BytePositionInLine.Value + 1;
        return $"{baseMessage} (Line: {line}, Column: {column})";
    }

    private static JsonTreeNodeViewModel ToViewModel(JsonTreeNode node)
    {
        var viewModel = new JsonTreeNodeViewModel { Label = node.Label };
        foreach (var child in node.Children)
        {
            viewModel.Children.Add(ToViewModel(child));
        }

        return viewModel;
    }

    private static string BuildSettingsPath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(appData, "JsonEditor", "settings.json");
    }

    private static string BuildBackupPath(string filePath)
    {
        return filePath + ".autosave";
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
