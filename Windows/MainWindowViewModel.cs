using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaDialogs.Views;
using CommunityToolkit.Mvvm;
using NMS_EnglishAlienWordsMod_Avalonia.Logic;
using NMSEnglishAlienWordsMod_Avalonia.Properties;
using Octokit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Titanium;


namespace NMS_EnglishAlienWordsMod_Avalonia.Windows
{

	public class MainWindowViewModel : INotifyPropertyChanged
	{
		public MainWindowViewModel()
		{
			// Если нужно, можно инициализировать коллекции, флаги и прочее
			VersionList = new ObservableCollection<VersionItem>();
		}

		#region Field and properties

		public Action<string>? ShowErrorMessage;

		public string CreateButtonText => IsDownloadingMbinc ? "Downloading..." :
								  SelectedVersion == null ? "Select MBINCompiler version" :
								  MbinCompilerManager.IsDownloaded(SelectedVersion.VersionName) ? "Create mod" : "Download MBINCompiler";
		public bool ButtonActive => SelectedVersion != null && isGamePathValid;
		private bool isGamePathValid => !Validator.TryValidateProperty(
        AppProperties.CurrentSettings.NoMansSkyGamePath,
        new ValidationContext(AppProperties.CurrentSettings) { MemberName = nameof(AppProperties.CurrentSettings.NoMansSkyGamePath) },
        new List<ValidationResult>());


		public SettingsObject Settings  => AppProperties.CurrentSettings;
		public int Progress { get; set; } = 0;

		#endregion Field and properties
		

		#region Version droplist
		public ObservableCollection<VersionItem> VersionList { get; } = new();
				private VersionItem _selectedVersion;
		public VersionItem SelectedVersion
		{
			get => _selectedVersion;
			set
			{
				if (_selectedVersion != value) {
					_selectedVersion = value;
					OnPropertyChanged(nameof(SelectedVersion));
					OnPropertyChanged(nameof(CreateButtonText));
				}
			}
		}
		public class VersionItem
		{
			public string VersionName { get; set; }
			public string DownloadUri { get; set; }
			public AvailabilityStatus FileAvailabilityStatus { get; set; }

			public VersionItem(string versionName, string downloadUri, AvailabilityStatus fileAvailabilityStatus)
			{
				VersionName = versionName;
				DownloadUri = downloadUri;
				FileAvailabilityStatus = fileAvailabilityStatus;
			
			}
		}

		public enum AvailabilityStatus
		{
			Unset, //. Just in case, should not be used
			NotDownloaded, //. Version avaible on Github, not downloaded
			Downloaded, //. Version avaible on Github, downloaded
			LocalOnly, //. Version not avaible on Github, only locally
		}

		/// <summary>
		/// Adds a new version or updates the availability status of an existing version.
		/// </summary>
		/// <param name="newItem"></param>
		public void AddOrUpdateVersion(VersionItem newItem)
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
		#endregion Version droplist

		public event PropertyChangedEventHandler? PropertyChanged;

		protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

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
					OnPropertyChanged(nameof(IsLoadingVersionList));
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
					OnPropertyChanged(nameof(IsDownloadingMbinc));
					OnPropertyChanged(nameof(CreateButtonText));
				}
			}
		}

		


		// Manages the content of cbMBINCompilerVersion combobox, serialization, interface-only. Release contents version and assets download link
		#region Release management
		private static string MBINCompilerReleasesFilePath = "MBINCompilerReleases.json";
		public async Task GetReleasesAsync()
		{
			await LoadReleasesFromFileAsync();
			if (VersionList == null || VersionList.Count == 0) {
				await UpdateReleasesOnlineAsync();
			}

		}
		// Updates the list of MBINCompiler releases versions from GitHub and saves it to a local file.
		public async Task UpdateReleasesOnlineAsync()
		{
			IsLoadingVersionList = true; //. UI spinner on
			try {
				var releases = await GitHub.GetAllReleasesAsync("monkeyman192", "MBINCompiler"); //. get release list from GitHub
				foreach (var release in releases) {
					AddOrUpdateVersion(
						new VersionItem(
					release.TagName,
					release.Assets.Where(a => a.Name.EndsWith(AppProperties.CurrentSettings.MbinCompilerAssetName)).FirstOrDefault()?.BrowserDownloadUrl ?? string.Empty,
					AvailabilityStatus.NotDownloaded
						)
					);

				}
				OnPropertyChanged(nameof(VersionList)); //. notify UI of change
				await SaveReleasesToFileAsync(); //. save to local file
			}
			catch (Exception ex) {
				SingleActionDialog dialog = new() { Message = $"Error getting MBINCompiler versions: {ex.Message}", ButtonText = "Ok" };
				await dialog.ShowAsync();
			}
			finally {
				IsLoadingVersionList = false; //. UI spinner off
			}
		}

		public async Task UpdateReleasesLocalAsync()
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
		public async Task SaveReleasesToFileAsync()
		{
			if (VersionList == null || VersionList.Count == 0)
				return;


			using FileStream createStream = File.Create(MBINCompilerReleasesFilePath, 4096, FileOptions.Asynchronous);
			await JsonSerializer.SerializeAsync(createStream, VersionList, jsonSerializerOptions);
		}

		// Loads the list of releases from a local JSON file.
		public async Task LoadReleasesFromFileAsync()
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