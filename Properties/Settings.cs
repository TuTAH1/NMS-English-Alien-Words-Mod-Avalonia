using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;


namespace NMSEnglishAlienWordsMod_Avalonia.Properties
{
	public class AppSettings
	{
		public string GameFolder;
	}

	public static class SettingsManager
	{
		private static readonly string settingsPath = 
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyApp", "settings.json");

		public static void Save(this AppSettings settings)
		{
			Directory.CreateDirectory(Path.GetDirectoryName(settingsPath)!);
			File.WriteAllText(settingsPath, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
		}

		public static AppSettings Load()
		{
			if (File.Exists(settingsPath))
			{
				return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(settingsPath)) ?? new AppSettings();
			}
			return new AppSettings();
		}

		public static void Load(this AppSettings settings)
		{
			settings = Load();
		}
	}

}
