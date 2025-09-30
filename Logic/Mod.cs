using NMS_EnglishAlienWordsMod_Avalonia.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NMS_EnglishAlienWordsMod_Avalonia.Logic
{
	

	internal partial class Mod
	{
		static string _languagesRegex = AppGlobals.CurrentSettings.languagesRegex.ToLower();
		static string[] _languagesList;
		static List<string> _LanguagesList => AppGlobals.CurrentSettings.Languages; //. List of Property names of languages
		//string languagesListPath = "Content/LanguagesList.txt";

		//private string[] SetLanguagesList() => _languagesList??= GetLanguagesList();
		//private string[] GetLanguagesList() => File.ReadAllLines(languagesListPath);
		static string _pakPath => AppGlobals.CurrentSettings.GetPakTargetFullPath();
		private static async Task AddProgressMessage(string? message = null, int increment = 0, int? percent = null)
		{
			if (message !=null) {
				AppGlobals.MessageBuffer.AddText("## ");
				AppGlobals.MessageBuffer.AddLine(message, AppGlobals.MessageBuffer.MessageType.Good);
			}

			// Report to current context
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

				await AddProgressMessage("Starting alien word extraction...");
				var dictionary = await XmlComposer.ExtractAlienWordsAsync();

				AppGlobals.MessageBuffer.AddLine("Starting mxml generation...");
				var mxmlContent = await XmlComposer.GenerateMxmlFileAsync(dictionary);

				AppGlobals.MessageBuffer.AddLine("Processing completed", AppGlobals.MessageBuffer.MessageType.Good);

				await XmlComposer.SaveMxmlToFileAsync(mxmlContent);
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
}
}
