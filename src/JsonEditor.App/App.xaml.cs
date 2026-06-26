using System.Windows;
using JsonEditor.App.Infrastructure;
using JsonEditor.App.ViewModels;
using JsonEditor.Core.Services;

namespace JsonEditor.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
	private MainWindow? _mainWindow;

	protected override async void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);
		var mainViewModel = new MainViewModel(
			new JsonValidationService(),
			new JsonTreeBuilder(),
			new SearchReplaceService(),
			new AppSettingsStore(),
			new FileDialogService(),
			new MessageBoxService());

		_mainWindow = new MainWindow
		{
			DataContext = mainViewModel
		};
		_mainWindow.Show();

		if (_mainWindow.DataContext is MainViewModel vm)
		{
			await vm.InitializeAsync();
			_mainWindow.ApplyTheme(vm.IsDarkTheme);
		}
	}

	protected override async void OnExit(ExitEventArgs e)
	{
		if (_mainWindow?.DataContext is MainViewModel vm)
		{
			await vm.ShutdownAsync();
		}

		base.OnExit(e);
	}
}

