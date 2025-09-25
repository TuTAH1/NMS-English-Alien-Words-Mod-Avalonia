using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static NMS_EnglishAlienWordsMod_Avalonia.Windows.MainWindowViewModel;
using FileMode = System.IO.FileMode;

namespace NMS_EnglishAlienWordsMod_Avalonia.Logic
{
	internal class MbinCompilerManager
	{
		public static string MbinCompilerPath = "MbinCompilers\\";
		public static string MbinCompilerAssetName = AppProperties.CurrentSettings.MbinCompilerAssetName;
		public string versionName;
		public static async Task DownloadAsync(string versionName, string downloadUri)
		{
			try
			{
				using var client = new HttpClient();
				var response = await client.GetAsync(downloadUri);
				response.EnsureSuccessStatusCode();
				string filePath = $"{MbinCompilerPath}{versionName}\\{MbinCompilerAssetName}";

				Directory.CreateDirectory(Path.GetDirectoryName(filePath));

				using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
				await response.Content.CopyToAsync(fileStream);
			}
			catch (Exception ex)
			{
				throw new Exception($"Error downloading MBINCompiler version {versionName}: {ex.Message}", ex);
			}
		}
		public static bool IsDownloaded(string version)
		{
			string path =  $"{MbinCompilerPath}{version}\\{MbinCompilerAssetName}";
			if (System.IO.File.Exists(path))
			{
				return true;
			}
			return false;
		}

		public static List<VersionItem> GetLocalVersions()
		{
			List<VersionItem> versions = new();
			if (Directory.Exists(MbinCompilerPath))
			{
				var dirs = Directory.GetDirectories(MbinCompilerPath);
				foreach (var dir in dirs)
				{
					string version = Path.GetFileName(dir); //. folder name is version
					if (IsDownloaded(version))
					{
						versions.Add(new VersionItem(
							version,
							null,
							AvailabilityStatus.LocalOnly
							));
					}
				}
			}
			return versions;
		}
	}
}