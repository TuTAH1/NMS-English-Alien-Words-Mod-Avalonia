using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.PropertyGrid.Controls;
using AvaloniaDialogs.Views;
using NMS_EnglishAlienWordsMod_Avalonia.Logic;
using NMS_EnglishAlienWordsMod_Avalonia.Windows;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using static NMS_EnglishAlienWordsMod_Avalonia.Logic.App;
using static NMS_EnglishAlienWordsMod_Avalonia.Windows.MainWindowViewModel;

namespace NMS_EnglishAlienWordsMod_Avalonia
{
	public partial class MainWindow : Window
	{
		private MainWindowViewModel _vm;

		public MainWindow()
		{
			InitializeComponent();	
			
			DataContext = SettingsModel;
		}

		#region Window Events

		protected override async void OnOpened(EventArgs e)
		{
			base.OnOpened(e);
			if (Design.IsDesignMode) return; //skip logic if in design mode
		
			await InitializeEverything();
			this.Loaded += OnLoadedAsync;
		
		}

		// Loads the available versions of MBINCompiler when the window is loaded.
		private async void OnLoadedAsync(object sender, EventArgs e)
		{
			try
			{
				await _vm.UpdateReleasesLocalAsync();
				await _vm.GetReleasesAsync();
			}
			catch(Exception ex)
			{
				_vm.ShowErrorMessage?.Invoke($"Failed to load releases: {ex.Message}");
			}
		}

		//. Essential settings category filter
		private void OnCustomPropertyDescriptorFilter(object sender, RoutedEventArgs args)
		{
			if (args is CustomPropertyDescriptorFilterEventArgs { TargetObject: SettingsObject} e)
			{
				if (e.PropertyDescriptor.Category == "Essential") 
					e.IsVisible = true;
				 else e.IsVisible = false;
				
				e.Handled = true;
			}
		}
		#endregion Window Events

		#region Initialization Methods
		//. Initializes everything. What do you mean it's not how you name methods??
		private async Task InitializeEverything()
		{
			InitializeDebugTools();
			CreateErrorDialogHost();
			InitializeViewModel();
			SetWindowMinSize();
			ClearConsole();
		}

		//. Adding Error Dialog to main grid
		private void CreateErrorDialogHost()
		{
			var host = new ReactiveDialogHost
			{
				CloseOnClickAway = true,
				HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
				VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
			};

			var placeholder = this.FindControl<ContentControl>("DialogHostPlaceholder");
			if (placeholder != null)
				placeholder.Content = host;
		}

		private void InitializeDebugTools()
		{
			#if DEBUG
			this.AttachDevTools();
			#endif
		}

		private void InitializeViewModel()
		{
			_vm = new MainWindowViewModel();
			DataContext = _vm;
			_vm.IsDownloadingMbinc = false;
			_vm.IsLoadingVersionList = false;
		}

		private void SetWindowMinSize()
		{
			//may be calculated, но мне лень
			this.MinWidth = 485;
			this.MinHeight = 183;
		}

		private void ClearConsole()
		{
			//#if  !DEBUG
			Console.Markdown = "";
			//#endif
		}

		#endregion Initialization Methods

		#region ControlsEventHandlers
		private void btnMBINC_CheckUpdates_Click(object? sender, RoutedEventArgs e)
		{
			_vm.UpdateReleasesOnlineAsync();
		}

		private CancellationTokenSource? _cts;
		private async void ButtonCreate_Click(object? sender, RoutedEventArgs e)
		{
			Console.Markdown = "";
			_cts = new CancellationTokenSource();
			Task modCreation = Task.CompletedTask;

			var progress = new Progress<ProgressReport>(report =>
			{
				if (report.Percent.HasValue)
					_vm.Progress = report.Percent.Value;
				else if (report.Increment.HasValue)
					_vm.Progress = Math.Clamp(_vm.Progress + report.Increment.Value, 0, 100);

				if (!string.IsNullOrEmpty(report.Message))
					_vm.ProgressText = report.Message;

				var chunk = MessageBuffer.GetAndClear();
				if (!string.IsNullOrEmpty(chunk))
					Console.Markdown += chunk;

			});

			try {
				if (cbMBINCompilerVersion.SelectedItem == null) {
					SingleActionDialog dialog = new() { Message = "Please select a version of MBINCompiler", ButtonText = "Ok" };
					return;
				}

				if(!MbinCompilerManager.IsDownloaded(MbincSelectedVersion.VersionName))
				{
					try {
						_vm.IsDownloadingMbinc = true;
						await MbinCompilerManager.DownloadAsync(MbincSelectedVersion.VersionName, MbincSelectedVersion.DownloadUri);
					}
					catch(Exception ex) {
						_vm.ShowErrorMessage?.Invoke($"Failed to download MBINCompiler: {ex.Message}");
					}
					finally {
						_vm.IsDownloadingMbinc = false;
					}
					return;
				}

			//### Creating mod
				modCreation = Mod.Create(progress, _cts.Token);

			} catch (Exception ex) {
				_vm.IsDownloadingMbinc = false;
				MessageBuffer.AddLine(ex);
				Logic.App.ErrorWindow = new() { Message = $"Error: {ex.Message}", ButtonText = "Ok" };
			}
			finally {
				try {
					await modCreation;
				}
				catch (OperationCanceledException) {
					MessageBuffer.AddLine("Cancelled");
				}
				catch (Exception ex) {
					MessageBuffer.AddLine($"Error: {ex.Message}");
				}
				Console.Markdown += MessageBuffer.GetAndClear();

				ShowError();
			}
		}

		private void btnSettings_Click(object? sender, RoutedEventArgs e)
		{
			Settings settings = new();
			settings.Show();
		}

		private void ComboboxMBINCompilerVersion_SelectionChanged(object? sender, SelectionChangedEventArgs e)
		{
			btnCreate.IsEnabled = MbincSelectedVersion != null;
		}

		#endregion ControlsEventHandlers

		#region Window methods



		#endregion Window methods
		private VersionItem MbincSelectedVersion => ((cbMBINCompilerVersion.SelectedItem) as VersionItem);
			
	}
}