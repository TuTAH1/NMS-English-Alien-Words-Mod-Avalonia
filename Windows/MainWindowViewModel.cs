using AvaloniaDialogs.Views;
using NMS_EnglishAlienWordsMod_Avalonia.Logic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using Titanium;
using Tmds.DBus.Protocol;


namespace NMS_EnglishAlienWordsMod_Avalonia.Windows
{
	public enum ProgressSuccessState
	{
		Unset,
		Success,
		Warning,
		Error
	}

	public class MainWindowViewModel : INotifyPropertyChanged
	{
		public MainWindowViewModel()
		{
			// Если нужно, можно инициализировать коллекции, флаги и прочее
			VersionList = new ObservableCollection<VersionItem>();
			Settings.PropertyChanged += Settings_PropertyChanged;
		}

		private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(Settings.NoMansSkyGamePath)) {
				OnPropertyChanged(nameof(ButtonEnabled));
			}
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#region Field and properties
		public string AppName => AppGlobals.AppName;

		public Action<string>? ShowErrorMessage;
		public string CreateButtonText => IsDownloadingMbinc ? "Downloading..." :
										  SelectedVersion == null ? "Select MBINCompiler version" :
										  MbinCompilerManager.IsDownloaded(SelectedVersion.VersionName) ? "Create mod" : "Download MBINCompiler";
		public bool ButtonEnabled => SelectedVersion != null && (isGamePathValid || !MbinCompilerManager.IsDownloaded(SelectedVersion.VersionName));
		private bool isGamePathValid
		{
			get
			{
				var results = new List<ValidationResult>();
				bool isValid = Validator.TryValidateProperty(
					Settings.NoMansSkyGamePath,
					new ValidationContext(Settings) { MemberName = nameof(Settings.NoMansSkyGamePath) },
					results);

				return isValid;
			}
		}


		public SettingsObject Settings  => AppGlobals.CurrentSettings;
		
		private int _progress;
		public int Progress
		{
			get => _progress;
			set
			{
				if (_progress != value)
				{
					_progress = value;
					OnPropertyChanged();
					OnPropertyChanged(nameof(IsProgressbarVisible));
				}
			}
		}

		private string _progressText = string.Empty;
		public string ProgressText
		{
			get => _progressText;
			set
			{
				if (_progressText != value)
				{
					_progressText = value;
					OnPropertyChanged();
				}
			}
		}

		private ProgressSuccessState _progressState = ProgressSuccessState.Unset;
		public ProgressSuccessState ProgressState
		{
			get => _progressState;
			set
			{
				if (_progressState != value) {
					_progressState = value;
					OnPropertyChanged();
				}
			}
		}

		public bool IsProgressbarVisible => Progress > 0;

		#endregion Field and properties


		#region Version droplist
		public ObservableCollection<VersionItem> VersionList { get; private set; } = new();
		private VersionItem? _selectedVersion;
		public VersionItem? SelectedVersion
		{
			get => _selectedVersion;
			set
			{
				if (_selectedVersion != value) {
					_selectedVersion = value;
					OnPropertyChanged();
					OnPropertyChanged(nameof(CreateButtonText));
					OnPropertyChanged(nameof(ButtonEnabled));
				}
			}
		}
		public class VersionItem
		{
			public string VersionName { get; set; }
			public string DownloadUri { get; set; }
			public AvailabilityStatus FileAvailabilityStatus { get; set; }
			public bool CanBeDeleted => FileAvailabilityStatus is AvailabilityStatus.Downloaded or AvailabilityStatus.LocalOnly;

			public VersionItem(string versionName, string downloadUri, AvailabilityStatus fileAvailabilityStatus)
			{
				VersionName = versionName;
				DownloadUri = downloadUri;
				FileAvailabilityStatus = fileAvailabilityStatus;
			
			}
		}

		public enum AvailabilityStatus
		{
			/// <summary>
			/// Just in case, should not be used
			/// </summary>
			Unset,
			/// <summary>
			/// Version avaible on Github, but not locally (not downloaded)
			/// </summary>
			NotDownloaded,
			/// <summary>
			/// Version avaible on Github and locally (downloaded)
			/// </summary>
			Downloaded,
			/// <summary>
			/// Version not avaible on Github, only locally
			/// </summary>
			LocalOnly,
		}

		/// <summary>
		/// Adds a new version or updates the availability status of an existing version.
		/// </summary>
		/// <param name="newItem"></param>
		private void AddOrUpdateVersion(VersionItem newItem)
		{
			//if (VersionList == null) VersionList = new ObservableCollection<VersionItem>();

			var existing = VersionList.FirstOrDefault(v => v.VersionName == newItem.VersionName);
			if (existing == null) //? new version
			{
				VersionList.Add(newItem);
				return;
			}
			//: set DownloadUrl if it's unset
			if (string.IsNullOrEmpty(existing.DownloadUri) && !string.IsNullOrEmpty(newItem.DownloadUri)) 
				existing.DownloadUri = newItem.DownloadUri;

			//: existing version, update availability status if needed
			switch ((existing.FileAvailabilityStatus, newItem.FileAvailabilityStatus))
			{
				case (AvailabilityStatus.Unset, _): //? unset -> any (just in case, should not happen)
				existing.FileAvailabilityStatus = newItem.FileAvailabilityStatus;
					OnPropertyChanged(nameof(VersionList));
					break;

				case (AvailabilityStatus.NotDownloaded, AvailabilityStatus.LocalOnly):
				case (AvailabilityStatus.LocalOnly, AvailabilityStatus.NotDownloaded):
					existing.FileAvailabilityStatus = AvailabilityStatus.Downloaded;
					OnPropertyChanged(nameof(VersionList));
					break;

			}
		}

		/// <summary>
		/// Removes a version locally and handles corresponding UI updates.
		/// </summary>
		/// <param name="item">version that should be deleted. Should be VersionList's item</param>
		/// <exception cref="Exception"></exception>
		public void RemoveVersion(VersionItem item)
		{
			if (item == null || !item.CanBeDeleted) return;

			var currentItem = VersionList.FirstOrDefault(item);
			if (currentItem == null) throw new Exception("View model error: trying to delete a version that is not in the list");

			try {
				

				MbinCompilerManager.DeleteVersion(item.VersionName);

				//! Change availability status or remove from list
				if (currentItem.FileAvailabilityStatus == AvailabilityStatus.Downloaded) //. if it's avaible online, it shouldn't be removed from list
					currentItem.FileAvailabilityStatus = AvailabilityStatus.NotDownloaded;
				else if(currentItem.FileAvailabilityStatus == AvailabilityStatus.LocalOnly)
					VersionList.Remove(currentItem);
				else
					throw new Exception("View movel error: trying to delete a version that is not marked as existing");

				RefreshReleasesAsync();

				if (SelectedVersion == currentItem)
					SelectedVersion = null;
			}
			catch (Exception ex) {
				throw new Exception($"Error deleting MBINCompiler version {currentItem.VersionName}: {ex.Message}", ex);
			}
		}
		#endregion Version droplist


		//mbinc check versions status for [version combobox]'s spinner
		private bool _isLoadingVersionList;
		//mbinc check versions status for [version combobox]'s spinner
		public bool IsLoadingVersionList
		{
			get => _isLoadingVersionList;
			set
			{
				if (_isLoadingVersionList != value) {
					_isLoadingVersionList = value;
					OnPropertyChanged();
					OnPropertyChanged(nameof(CreateButtonText));
				}
			}
		}
		//test 

		//mbinc downloading status for [create mod button]'s spinner
		private bool _isDownloadingMbinc;
		public bool IsDownloadingMbinc
		{
			get => _isDownloadingMbinc;
			set
			{
				if (_isDownloadingMbinc != value) {
					_isDownloadingMbinc = value;
					OnPropertyChanged();
					OnPropertyChanged(nameof(CreateButtonText));
				}
			}
		}




		// Manages the content of cbMBINCompilerVersion combobox, serialization, interface-only. Release contents version and assets download link
		#region Release management
		private static string MBINCompilerReleasesFilePath = "MBINCompilerReleases.json";
		public async Task RefreshReleasesAsync(bool getUpdates = false)
		{
			VersionList = new();

			if(!getUpdates)
				await LoadOnlineReleasesFromFileAsync();

			if ( VersionList == null || VersionList.Count == 0)
				await UpdateReleasesOnlineAsync();

			await UpdateReleasesLocalAsync();
		}
		// Updates the list of MBINCompiler releases versions from GitHub and saves it to a local file.
		private async Task UpdateReleasesOnlineAsync()
		{
			IsLoadingVersionList = true; //. UI spinner on
			try {
				var releases = await GitHub.GetAllReleasesAsync("monkeyman192", "MBINCompiler"); //. get release list from GitHub
				var versionItems = releases.Select(r => new VersionItem(
					r.TagName,
					r.Assets.Where(a => a.Name.EndsWith(Settings.MbinCompilerAssetName)).FirstOrDefault()?.BrowserDownloadUrl ?? string.Empty,
					AvailabilityStatus.NotDownloaded
					)).ToList();

				await SaveOnlineReleasesToFileAsync(versionItems); //. save to local file

				foreach (var versionItem in versionItems) {
					AddOrUpdateVersion(versionItem);

				}
				OnPropertyChanged(nameof(VersionList)); //. notify UI of change
			}
			catch (Exception ex) {
				SingleActionDialog dialog = new() { Message = $"Error getting MBINCompiler versions: {ex.Message}", ButtonText = "Ok" };
				AppGlobals.ErrorWindow = dialog;
				AppGlobals.MessageBuffer.AddLine(ex);
			}
			finally {
				IsLoadingVersionList = false; //. UI spinner off
			}
		}
		private async Task UpdateReleasesLocalAsync()
		{
			List<VersionItem> localReleases = MbinCompilerManager.GetLocalVersions();
			localReleases.ForEach(release => AddOrUpdateVersion(release));
		}

		private  JsonSerializerOptions jsonSerializerOptions = new ()
		{
			PropertyNameCaseInsensitive = true,
			IgnoreNullValues = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			IncludeFields = true
		};

		// Saves the current list of releases to a local JSON file.
		private async Task SaveOnlineReleasesToFileAsync(List<VersionItem> versionItems)
		{
			if (versionItems == null || versionItems.Count == 0)
				return;


			using FileStream createStream = File.Create(MBINCompilerReleasesFilePath, 4096, FileOptions.Asynchronous);
			await JsonSerializer.SerializeAsync(createStream, versionItems, jsonSerializerOptions);
		}

		// Loads the list of releases from a local JSON file.
		private async Task LoadOnlineReleasesFromFileAsync()
		{
			try 
			{
				if (!File.Exists(MBINCompilerReleasesFilePath))
					return;

				using FileStream openStream = File.OpenRead(MBINCompilerReleasesFilePath);
				ObservableCollection<VersionItem>? versionList = await JsonSerializer.DeserializeAsync<ObservableCollection<VersionItem>>(openStream, jsonSerializerOptions);

				if (versionList != null) {
					versionList.ToList().ForEach(version => AddOrUpdateVersion(version));
					OnPropertyChanged(nameof(VersionList));
				}
			}
			catch (Exception ex) {
				SingleActionDialog dialog = new() { Message = $"Error loading MBINCompiler versionlist (nothing serious):\n {ex.Message}", ButtonText = "Fine, I'll just refresh it" };
				await dialog.ShowAsync();
			}
		}

		#endregion Release management



	}
}