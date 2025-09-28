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
		static string _languagesRegex = AppGlobals.CurrentSettings.languagesRegex.ToLower();
		static string[] _languagesList;
		static List<string> _LanguagesList => AppGlobals.CurrentSettings.Languages; //. List of Property names of languages
		//string languagesListPath = "Content/LanguagesList.txt";

		//private string[] SetLanguagesList() => _languagesList??= GetLanguagesList();
		//private string[] GetLanguagesList() => File.ReadAllLines(languagesListPath);
		static string _pakPath = AppGlobals.CurrentSettings.GetPakTargetFullPath();
		private static async Task AddProgressMessage(string message = null, int increment = 0, int? percent = null)
		{
			if (message !=null) {
				AppGlobals.MessageBuffer.AddText("## ");
				AppGlobals.MessageBuffer.AddLine(message, AppGlobals.MessageBuffer.MessageType.Good);
			}

			// Репортим в текущий контекст, если он задан
			var progress = AppGlobals.ProgressContext.Current;
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
			using (var _ = AppGlobals.ProgressContext.Set(Progress))
			{

				await AddProgressMessage("Listing pak content", percent: 0);

				await HgpakTool.ListPakContents();
				await AddProgressMessage("Pak contents listed", 0);
				if (AppGlobals.CurrentSettings.StopAfter <=SettingsObject.DebugStopPoint.FilelistJson) return;
			
				await AddProgressMessage("Filtering Json Filelist");
				await HgpakTool.CreateFilteredJsonFilelist();
				await AddProgressMessage("Filtered Json Filelist Created", 2);
				if (AppGlobals.CurrentSettings.StopAfter <= SettingsObject.DebugStopPoint.changedFilelistJson) return;
			
				await AddProgressMessage("Unpacking Bin files");
				await HgpakTool.UnpackBin();
				await AddProgressMessage("Bin unpacked", 2);
				if (AppGlobals.CurrentSettings.StopAfter <= SettingsObject.DebugStopPoint.UnpackBin) return;

				await AddProgressMessage("Unpacking MBIN files to XML");
				await MbinCompiler.UnpackAllMbins();
				await AddProgressMessage("MBIN files converted to XML", 2);
				if (AppGlobals.CurrentSettings.StopAfter <= SettingsObject.DebugStopPoint.UnpackMbins) return;
			}
					
		}

		
		/// <summary>
		/// Log the output and error streams of a process asynchronously, updating progress periodically.
		/// </summary>
		/// <param name="process"> The process to log. </param>
		/// <param name="progressPercent"> The total percentage of progress to allocate for this process. </param>
		/// <returns></returns>
		private static async Task LogProcessAsync(Process process, int progressPercent, int estimatedTimeMs = 1000)
		{
			int delayPerStep = 50; //. 20 FPS, but it's fine with smoothing (easing) in UI
			int totalSteps = Math.Max(1, estimatedTimeMs / delayPerStep);
			double incrementPerStep = progressPercent / (double)totalSteps;


			//. Reading console output
			var outputTask = process.StandardOutput.ReadToEndAsync();
			var errorTask = process.StandardError.ReadToEndAsync();
		
			

			//. Progress animation while waiting for process to exit
			int currentStep = 0;
			while (!process.HasExited && currentStep < totalSteps)
			{
				await AddProgressMessage(null, (int)Math.Ceiling(incrementPerStep));

				//. Give UI thread a breath
				await Task.Delay(delayPerStep);
			
				currentStep++;
			}

			//. Ensure we reach the full allocated progress
			await AddProgressMessage(null,(int)Math.Ceiling((double)(totalSteps-currentStep)));

			//. Get results
			string output = await outputTask;
			string error = await errorTask;
		
			AppGlobals.MessageBuffer.AddLine(output);
			if (!string.IsNullOrEmpty(error))
				AppGlobals.MessageBuffer.AddLine(error, AppGlobals.MessageBuffer.MessageType.Error);
		}
		

		/// <summary>
		/// Wrapper for HGPakTool.exe functionality
		/// Unpacks needed English language .mbin files from the specified .pak file by specified languagesRegex
		/// </summary>
		private static class HgpakTool
		{
			static string toolPath = "HGPakTool.exe";
			static string workingDir = "Content";
			static string toolFullPath = Path.Combine(workingDir, toolPath);
			static string filelistJsonPath = $"{workingDir}/filenames.json";
			static string filteredFilelistJsonName = $"FilteredFilenames.json";
			static ProcessStartInfo toolStartInfo = new ProcessStartInfo()
			{
				FileName = toolFullPath,
				WorkingDirectory = workingDir,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false
			};

			/// <summary>
			/// List the contents of the specified .pak
			/// </summary>
			/// <returns></returns>
			/// <exception cref="FileNotFoundException"></exception>
			public static async Task ListPakContents()
			{
				await Task.Run(async () => 
				{
					if(!File.Exists(toolFullPath))
						throw new FileNotFoundException("HGPakTool not found.");
				
					toolStartInfo.Arguments = $"-L \"{_pakPath}\"";
					var process = Process.Start(toolStartInfo)!;
			
					await LogProcessAsync(process, 5);
				});
			}

			/// <summary>
			/// Create a <b>filteredFilenames.json file</b> list containing only files that match the specified language regex, using the existing <b>filenames.json</b> file.
			/// </summary>
			/// <returns></returns>
			/// <exception cref="FileNotFoundException">filenames.json not found</exception>
			/// <exception cref="NullReferenceException">filenames.json wasn't deserialized correctly</exception>

			/// <exception cref="Exception"></exception>
			public static async Task CreateFilteredJsonFilelist()
			{
				await Task.Run(async () => 
				{
					if (!File.Exists(filelistJsonPath))
						await ListPakContents();
        
					if (!File.Exists(filelistJsonPath) && !File.Exists(filteredFilelistJsonName))
						throw new FileNotFoundException("filenames.json wasn't created by HGPakTool. Try creating filteredFilenames.json manually");
        
					try {
						await AddProgressMessage(null, 2);
            
						var json = await File.ReadAllTextAsync(filelistJsonPath);
						var files = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
						var pakPath = files.Keys.First();
						var filesOfThePak = files[pakPath];
            
						if(filesOfThePak == null)
							throw new NullReferenceException("filesOfThePak is null");
            
						await AddProgressMessage(null, 3);
            
						List<string> filteredFiles = filesOfThePak.Where(file => 
							Regex.IsMatch(file, _languagesRegex)).ToList();
						files[pakPath] = filteredFiles;
            
						await AddProgressMessage(null, 3);
            
						await File.WriteAllTextAsync(filteredFilelistJsonName, JsonConvert.SerializeObject(files, Formatting.Indented));
					}
					catch (Exception ex) {
						throw new Exception("Error while creating filteredFilenames.json", ex);
					}
					finally {
						if(File.Exists(filelistJsonPath))
							File.Delete(filelistJsonPath);
					}
				});
			}

			/// <summary>
			/// Unpack .mbin files from the specified .pak file, only ones that listed in the <b>filteredFilenames.json</b> file
			/// </summary>
			/// <returns></returns>
			/// <exception cref="FileNotFoundException">HGPakTool or filteredFilenames.json</exception>
			public static async Task UnpackBin()
			{
				await Task.Run(async () => 
				{
					if(!File.Exists(toolFullPath))
						throw new FileNotFoundException("HGPakTool not found.");
					if(!File.Exists(filteredFilelistJsonName))
						throw new FileNotFoundException("filteredFilenames.json not found. Can't unpack files without it.");

					toolStartInfo.Arguments = $"-j {filteredFilelistJsonName} -U \"{_pakPath}\"";
					var process = Process.Start(toolStartInfo)!;
			
					await LogProcessAsync(process, 10);
				});
			}
		}
		/// <summary>
		/// Wrapper for MBINCompiler functionality
		/// unpaks all .mbin files in the specified target directory to .xml files... that's all. I don't know why did I created this class just for 1 func
		/// </summary>
		private static class MbinCompiler
		{
			public static string TargetDirectoryPath => Path.Combine(Environment.CurrentDirectory,"Content","EXTRACTED\\language");
			private static ProcessStartInfo toolStartInfo = new ProcessStartInfo()
			{
				FileName = AppGlobals.Mbinc!.ExePath,
				WorkingDirectory = TargetDirectoryPath,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false
			};
			public static async Task UnpackAllMbins()
			{
				await Task.Run(async () => 
				{
					if(!File.Exists(toolStartInfo.FileName))
						throw new FileNotFoundException("MBINCompiler not found.");
				
					//? Check if there's mbin files in specified location
					if(!Directory.Exists(TargetDirectoryPath) || !Directory.EnumerateFiles(TargetDirectoryPath, "*.mbin", SearchOption.AllDirectories).Any())
						throw new FileNotFoundException("No .mbin files found to unpack. Make sure you unpacked the pak file with HGPakTool first.");
					toolStartInfo.Arguments = $"{TargetDirectoryPath} --input-format=MBIN"; //TODO: can be moved to settings, but it requires making a method for replacing keywords with settings variables 
					var process = Process.Start(toolStartInfo)!;
			
					await LogProcessAsync(process, 30);

					//: Clean up .mbin files after unpacking
					var mbinFiles = Directory.GetFiles(TargetDirectoryPath, "*.mbin", SearchOption.AllDirectories);
					foreach (var file in mbinFiles)
						File.Delete(file);
				});
			}
		}


	}
}
