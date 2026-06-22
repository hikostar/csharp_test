using System.Windows;
using JsonEditor.App.ViewModels;

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
		_mainWindow = new MainWindow();
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

