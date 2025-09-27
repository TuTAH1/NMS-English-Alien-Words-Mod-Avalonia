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
			static string toolPath = "HGPakTool.exe";
			static string workingDir = "Content";
			static string toolFullPath = Path.Combine(workingDir, toolPath);
			static string filelistJsonPath = $"{workingDir}/filenames.json";
			static string filteredFilelistJsonPath = $"{workingDir}/FilteredFilenames.json";
			static ProcessStartInfo toolStartInfo = new ProcessStartInfo()
			{
				FileName = toolFullPath,
				WorkingDirectory = workingDir,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false
			};

			/// <summary>
			/// Uses HGPakTool to create a "filenames.json" file of the contents in a pak file.
			/// </summary>
			public static async Task ListPakContents()
			{
				await Task.Run(async () => 
				{
					if(!File.Exists(toolFullPath))
						throw new FileNotFoundException("HGPakTool not found.");
				
					toolStartInfo.Arguments = $"-L \"{_pakPath}\"";
					var process = Process.Start(toolStartInfo)!;
			
					await LogProcessAsync(process, "Reading pak file");
				});
			}

			public static async Task CreateFilteredJsonFilelist()
			{
				await Task.Run(async () => 
				{
					if (!File.Exists(filelistJsonPath))
						await ListPakContents();
				
					if (!File.Exists(filelistJsonPath) && !File.Exists(filteredFilelistJsonPath))
						throw new FileNotFoundException("filenames.json wasn't created by HGPakTool. I don't know why. Try creating filteredFilenames.json manually");
			
					try {
						// Сообщаем о прогрессе
						ReportProgress("Reading JSON file", 12);
				
						var json = await File.ReadAllTextAsync(filelistJsonPath);
						var files = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
						List<string>? filesOfThePak = files?.First().Value;
				
						if(filesOfThePak == null)
							throw new NullReferenceException("filesOfThePak is null. Probably, something is wrong with your Regex.");
				
						ReportProgress("Filtering files", 14);
						List<string> filteredFiles = filesOfThePak.Where(file => Regex.IsMatch(file, _languagesRegex)).ToList();
						files.Remove(files.Keys.First());
						files.Add("FilteredFiles", filteredFiles);
				
						ReportProgress("Saving filtered list", 16);
						await File.WriteAllTextAsync(filteredFilelistJsonPath, JsonConvert.SerializeObject(files));
				
						ReportProgress("Filtered list saved", 18);
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

			public static async Task UnpackBin()
			{
				await Task.Run(async () => 
				{
					if(!File.Exists(Path.Combine(workingDir, toolPath)))
						throw new FileNotFoundException("HGPakTool not found.");
					
					toolStartInfo.Arguments = $"-j \"{filteredFilelistJsonPath}\" -U \"{_pakPath}\"";
					var process = Process.Start(toolStartInfo)!;
			
					await LogProcessAsync(process, "Unpacking bin files");
				});
			}

			private static async Task LogProcessAsync(Process process, string operationName)
			{
				// Создаем задачи для асинхронного чтения потоков вывода
				var outputTask = process.StandardOutput.ReadToEndAsync();
				var errorTask = process.StandardError.ReadToEndAsync();
		
				// Отслеживаем прогресс во время выполнения процесса
				int progressCounter = 0;
				while (!process.HasExited)
				{
					// Периодически обновляем прогресс
					ReportProgress($"{operationName}... {progressCounter}%");
			
					// Даем процессу поработать 100мс перед следующей проверкой
					if (await Task.WhenAny(Task.Delay(100), Task.Run(() => process.WaitForExit(100))) == Task.CompletedTask)
						break;
				
					// Увеличиваем счетчик прогресса (просто для визуализации)
					progressCounter = (progressCounter + 1) % 100;
				}
		
				// Получаем результаты асинхронного чтения
				string output = await outputTask;
				string error = await errorTask;
		
				App.MessageBuffer.AddLine(output);
				if (!string.IsNullOrEmpty(error))
					App.MessageBuffer.AddLine(error, App.MessageBuffer.MessageType.Error);
			}
	
			// Вспомогательный метод для отчета о прогрессе
			private static void ReportProgress(string message, int? percent = null)
			{
				var progress = App.ProgressContext.Current;
				if (progress != null)
				{
					progress.Report(new ProgressReport { 
						Message = message, 
						Percent = percent,
						Increment = percent.HasValue ? null : 1
					});
				}
			}
		}


	}
}
