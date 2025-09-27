using Newtonsoft.Json;
using NMS_EnglishAlienWordsMod_Avalonia.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace NMS_EnglishAlienWordsMod_Avalonia.Logic
{
	

	internal class Mod
	{
		static string _languagesRegex = App.CurrentSettings.languagesRegex.ToLower();
		static string[] _languagesList;
		static List<string> _LanguagesList => App.CurrentSettings.Languages; //. List of Property names of languages
		//string languagesListPath = "Content/LanguagesList.txt";

		//private string[] SetLanguagesList() => _languagesList??= GetLanguagesList();
		//private string[] GetLanguagesList() => File.ReadAllLines(languagesListPath);
		static string _pakPath = App.CurrentSettings.GetPakTargetFullPath();
		private static async Task AddProgressMessage(string message = null, int increment = 0, int? percent = null)
		{
			if (message !=null) {
				App.MessageBuffer.AddText("## ");
				App.MessageBuffer.AddLine(message, App.MessageBuffer.MessageType.Good);
			}

			// Репортим в текущий контекст, если он задан
			var progress = App.ProgressContext.Current;
			if (progress == null) return;

			var report = new ProgressReport
			{
				Message = message,
				Increment = increment,
				Percent = percent
			};

			progress.Report(report);
			await Task.Yield(); //Let the UI thread process the report
		}
		public static async Task Create(IProgress<ProgressReport>? Progress = null, CancellationToken Ct = default)
		{
			using (var _ = App.ProgressContext.Set(Progress))
			{

				await AddProgressMessage("Listing pak content", percent: 0);

				await HgpakTool.ListPakContents();
				await AddProgressMessage("Pak contents listed", 10);
				if (App.CurrentSettings.StopAfter <=SettingsObject.DebugStopPoint.FilelistJson) return;
			
				await AddProgressMessage("Filtering Json Filelist");
				await HgpakTool.CreateFilteredJsonFilelist();
				await AddProgressMessage("Filtered Json Filelist Created", 10);
				if (App.CurrentSettings.StopAfter <= SettingsObject.DebugStopPoint.changedFilelistJson) return;
			
				await AddProgressMessage("Unpacking Bin files");
				await HgpakTool.UnpackBin();
				await AddProgressMessage("Bin unpacked", 20);
				if (App.CurrentSettings.StopAfter <= SettingsObject.DebugStopPoint.UnpackBin) return;
			}
					
		}


		private static class HgpakTool
		{
			static string _toolPath = "HGPakTool.exe";
			static string _workingDir = "Content";
			static string _toolFullPath = Path.Combine(_workingDir,_toolPath);
			static string _filelistJsonPath = $"{_workingDir}/filenames.json";
			static string _filteredFilelistJsonPath = $"{_workingDir}/FilteredFilenames.json";
			static ProcessStartInfo toolStartInfo = new ProcessStartInfo()
			{
				FileName =  _toolFullPath,
				WorkingDirectory = _workingDir,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false
			};
			 


			/// <summary>
			/// Uses HGPakTool to create a "filenames.json" file of the contents in a pak file.	
			/// </summary>
			static public Task ListPakContents()
			{
				if(!File.Exists(_toolFullPath))
					throw new FileNotFoundException("HGPakTool not found.");
					
				toolStartInfo.Arguments = $"-L \"{_pakPath}\"";
				var process = Process.Start(toolStartInfo)!;
				LogProcess(process);
				process.WaitForExit();			
				
				return Task.CompletedTask;
			}

			static public Task CreateFilteredJsonFilelist()
			{
				if (!File.Exists(_filelistJsonPath))
					ListPakContents();
				if (!File.Exists(_filelistJsonPath) && !File.Exists(_filteredFilelistJsonPath)) //. filteredFilelistJsonPath left as a possibility for user to fix it manually
					throw new FileNotFoundException("filenames.json wasn't created by HGPakTool. I don't know why. Try creating filteredFilenames.json manually");
				
				try {
					var json = File.ReadAllText(_filelistJsonPath);
					var files = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
					List<string>? filesOfThePak = files?.First().Value;
					if(filesOfThePak == null)
						throw new NullReferenceException("filesOfThePak is null. Probably, something is wrong with your Regex.");
					List<string> filteredFiles = filesOfThePak.Where(file => Regex.IsMatch(file, _languagesRegex)).ToList();
					files.Remove(files.Keys.First());
					files.Add("FilteredFiles", filteredFiles);
					//\ App.MessageBuffer.AddLine($"Debug: 7th file is {filesOfThePak[6]}, regex is {_languagesRegex}, is match: {Regex.IsMatch(filesOfThePak[6], _languagesRegex)}");
					File.WriteAllText(_filteredFilelistJsonPath, JsonConvert.SerializeObject(files));
				}
				catch (Exception ex) {
					throw new Exception("Error while creating filteredFilenames.json", ex);
				}
				finally {
					if(File.Exists(_filelistJsonPath))
						File.Delete(_filelistJsonPath);
				}

				return Task.CompletedTask;
			}

			static public Task UnpackBin()
			{
				if(!File.Exists(Path.Combine(_workingDir,_toolPath)))
					throw new FileNotFoundException("HGPakTool not found.");
						
				toolStartInfo.Arguments = $"-j \"{_filteredFilelistJsonPath}\" -U \"{_pakPath}\"";
				var process = Process.Start(toolStartInfo)!;
				LogProcess(process);
				process.WaitForExit();	

				return Task.CompletedTask;
			}

			private static void LogProcess(Process process)
			{
				string output = process.StandardOutput.ReadToEnd();
				string error = process.StandardError.ReadToEnd();
				App.MessageBuffer.AddLine(output);
				App.MessageBuffer.AddLine(error, App.MessageBuffer.MessageType.Error);
				process.WaitForExit();	
			}
		
		}

	}
}
