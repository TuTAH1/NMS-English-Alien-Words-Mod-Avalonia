using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.PropertyGrid.Controls;
using AvaloniaDialogs.Views;
using NMS_EnglishAlienWordsMod_Avalonia.Logic;
using NMS_EnglishAlienWordsMod_Avalonia.Windows;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;
using static NMS_EnglishAlienWordsMod_Avalonia.Windows.MainWindowViewModel;

namespace NMS_EnglishAlienWordsMod_Avalonia
{
	public partial class MainWindow : Window
	{
		private MainWindowViewModel _vm = new();

		public MainWindow()
		{
			InitializeComponent();
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
				await _vm.RefreshReleasesAsync();
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
			InitializeViewModel();
			InitializeDebugTools();
			CreateErrorDialogHost();
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


		#endregion Initialization Methods

		#region Controls Event Handlers
		private async void btnMBINC_CheckUpdates_Click(object? sender, RoutedEventArgs e)
		{
			await _vm.RefreshReleasesAsync(true);
		}

		private void btnMBINC_Delete_Click(object? sender, RoutedEventArgs e)
		{
			if (MbincSelectedVersion != null && MbinCompilerManager.IsDownloaded(MbincSelectedVersion.VersionName))
			{
				try
				{
					var selectedVersion = MbincSelectedVersion;
					cbMBINCompilerVersion.SelectedItem = null;
					_vm.RemoveVersion(selectedVersion);



					MbinCompilerManager.DeleteVersion(selectedVersion.VersionName);
					AppGlobals.MessageBuffer.AddLine($"Deleted MBINCompiler version {selectedVersion.VersionName}", AppGlobals.MessageBuffer.MessageType.Good);
					UpdateConsole();
					
					ComboboxMBINCompilerVersion_SelectionChanged(null, null);
				}
				catch(Exception ex)
				{
					ConsoleWriteError(ex);
					UpdateConsole();
				}
			}
		}

		private CancellationTokenSource? _cts;
		private async void ButtonCreate_Click(object? sender, RoutedEventArgs e)
		{
			_cts = new CancellationTokenSource();
			Task modCreation = Task.CompletedTask;
			ClearConsole();

			var progress = new Progress<ProgressReport>(report =>
			{
				if (report.Percent.HasValue)
					_vm.Progress = report.Percent.Value;
				else if (report.Increment.HasValue)
					_vm.Progress = Math.Clamp(_vm.Progress + report.Increment.Value, 0, (int)ProgressBar.Maximum);

				if (!string.IsNullOrEmpty(report.Message))
					_vm.ProgressText = report.Message;

				UpdateConsole();
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
						_vm.RefreshReleasesAsync();
					}
					catch(Exception ex) {
						ConsoleWriteError(ex);
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
				ConsoleWriteError(ex);
				AppGlobals.ErrorWindow = new() { Message = $"Error: {ex.Message}", ButtonText = "Ok" };

			}
			finally {
				try {
					await modCreation;
				}
				catch (OperationCanceledException) {
					AppGlobals.MessageBuffer.AddLine("Cancelled", AppGlobals.MessageBuffer.MessageType.Error);
				}
				catch (Exception ex) {
					ConsoleWriteError(ex);
				}
				UpdateConsole();

				await Task.Run(async () => {await AppGlobals.ShowError();});
				_vm.ProgressText = AppGlobals.MessageBuffer.HighestMessageType switch
				{
					null => "Done",
					AppGlobals.MessageBuffer.MessageType.Info => "Completed",
					AppGlobals.MessageBuffer.MessageType.Good => "Completed successfully",
					AppGlobals.MessageBuffer.MessageType.Warn => "Completed with warnings",
					AppGlobals.MessageBuffer.MessageType.Error => "Error occurred",
					_ => "Done"
				};
			}
		}

		private void btnSettings_Click(object? sender, RoutedEventArgs e)
		{
			Settings settings = new();
			settings.Show();
		}

		private void ComboboxMBINCompilerVersion_SelectionChanged(object? sender, SelectionChangedEventArgs e)
		{
			AppGlobals.Mbinc = MbincSelectedVersion is null? null : new MbinCompilerManager { Version = MbincSelectedVersion.VersionName };
		}

		#endregion ControlsEventHandlers

		#region Window methods

		/// <summary>
		/// Updates the console with new messages from the message buffer and adjusts the progress state based on the highest message type.
		/// </summary>
		private void UpdateConsole()
		{
			var chunk = AppGlobals.MessageBuffer.GetAndClear();
			if (!string.IsNullOrEmpty(chunk))
				Console.Markdown += chunk;

			_vm.ProgressState = AppGlobals.MessageBuffer.HighestMessageType switch
			{
				AppGlobals.MessageBuffer.MessageType.Good => ProgressSuccessState.Success,
				AppGlobals.MessageBuffer.MessageType.Warn => ProgressSuccessState.Warning,
				AppGlobals.MessageBuffer.MessageType.Error => ProgressSuccessState.Error,
				_ => ProgressSuccessState.Unset
			};
		}

		private void ClearConsole()
		{
			Console.Markdown = "";
			AppGlobals.MessageBuffer.HighestMessageType = null;
		}

		private void ConsoleWriteError(Exception ex)
		{
			AppGlobals.MessageBuffer.AddLine(ex);
		}

		#endregion Window methods
		private VersionItem MbincSelectedVersion => ((cbMBINCompilerVersion.SelectedItem) as VersionItem);
			
	}
}