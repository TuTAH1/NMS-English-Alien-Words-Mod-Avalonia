using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.PropertyGrid.Controls;
using AvaloniaDialogs.Views;
using NMS_EnglishAlienWordsMod_Avalonia.Logic;
using NMS_EnglishAlienWordsMod_Avalonia.Windows;
using System;
using static NMS_EnglishAlienWordsMod_Avalonia.Logic.AppProperties;
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

		protected override void OnOpened(EventArgs e)
		{
			base.OnOpened(e);
			if (Design.IsDesignMode) return; //skip logic if in design mode
		
			InitializeEverything();
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
		private void InitializeEverything()
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
			#if  !DEBUG
			Console.Markdown = "";
			#endif
		}

		#endregion Initialization Methods

		#region ControlsEventHandlers
		private void btnMBINC_CheckUpdates_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			_vm.UpdateReleasesOnlineAsync();
		}

		private async void ButtonCreate_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			try {
				//: Check if MBINCompiler version is not selected
				if (cbMBINCompilerVersion.SelectedItem == null) {
					SingleActionDialog dialog = new() { Message = "Please select a version of MBINCompiler", ButtonText = "Ok" };
					return;
				}
				//: check if selected MBINCompiler version is not downloaded
				if(!MbinCompilerManager.IsDownloaded(MbincSelectedVersion.VersionName))
				{ //? NOT downloaded
					try
					{
						_vm.IsDownloadingMbinc = true;
						await MbinCompilerManager.DownloadAsync(MbincSelectedVersion.VersionName, MbincSelectedVersion.DownloadUri);
					}
					catch(Exception ex)
					{
						_vm.ShowErrorMessage?.Invoke($"Failed to download MBINCompiler: {ex.Message}");
					}
					finally
					{
						_vm.IsDownloadingMbinc = false;
					}

					return;
				}

				Mod.Create();

			} catch (Exception ex) {
				_vm.IsDownloadingMbinc = false;
				SingleActionDialog dialog  = new() { Message = $"Error: {ex.Message}", ButtonText = "Ok" };
				await dialog.ShowAsync();
			}
			finally {
				Console.Markdown = CurrentState.MessageBuffer.GetAndClear();
			}
		}

		private void btnSettings_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
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