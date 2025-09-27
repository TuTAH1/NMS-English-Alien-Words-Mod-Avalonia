using Newtonsoft.Json;
using NMS_EnglishAlienWordsMod_Avalonia.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using static NMS_EnglishAlienWordsMod_Avalonia.Logic.AppProperties;

namespace NMS_EnglishAlienWordsMod_Avalonia.Logic
{
	internal class Mod
	{
		static string _languagesRegex = CurrentSettings.languagesRegex;
		static string[] _languagesList;
		static List<string> _LanguagesList => CurrentSettings.Languages; //. List of Property names of languages
		//string languagesListPath = "Content/LanguagesList.txt";

		//private string[] SetLanguagesList() => _languagesList??= GetLanguagesList();
		//private string[] GetLanguagesList() => File.ReadAllLines(languagesListPath);
		static string _pakPath = CurrentSettings.GetPakTargetFullPath();

		public static void Create()
		{
			HgpakTool.ListPakContents();
			CurrentState.MessageBuffer.AddText("## "); CurrentState.MessageBuffer.AddLine("Pak contents listed", CurrentState.MessageBuffer.MessageType.Good);
			if (CurrentSettings.StopAfter <=SettingsObject.DebugStopPoint.FilelistJson) return;

			HgpakTool.CreateFilteredJsonFilelist();
			CurrentState.MessageBuffer.AddText("## "); CurrentState.MessageBuffer.AddLine("Filtered Json Filelist Created", CurrentState.MessageBuffer.MessageType.Good);
			if (CurrentSettings.StopAfter <= SettingsObject.DebugStopPoint.changedFilelistJson) return;
			
			HgpakTool.UnpackBin();
			CurrentState.MessageBuffer.AddText("## "); CurrentState.MessageBuffer.AddLine("Bin unpacked", CurrentState.MessageBuffer.MessageType.Good);
			if (CurrentSettings.StopAfter <= SettingsObject.DebugStopPoint.UnpackBin) return;
					
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
			static public void ListPakContents()
			{
				if(!File.Exists(_toolFullPath))
					throw new FileNotFoundException("HGPakTool not found.");
					
				toolStartInfo.Arguments = $"-L \"{_pakPath}\"";
				var process = Process.Start(toolStartInfo)!;
				string output = process.StandardOutput.ReadToEnd();
				string error = process.StandardError.ReadToEnd();
				CurrentState.MessageBuffer.AddLine(output);
				CurrentState.MessageBuffer.AddLine(error, CurrentState.MessageBuffer.MessageType.Error);
				process.WaitForExit();					
			}

			static public void CreateFilteredJsonFilelist()
			{
				if (!File.Exists(_filelistJsonPath))
					ListPakContents();
				if (!File.Exists(_filelistJsonPath) && !File.Exists(_filteredFilelistJsonPath)) //. filteredFilelistJsonPath left as a possibility for user to fix it manually
					throw new FileNotFoundException("filenames.json wasn't created by HGPakTool. I don't know why. Try creating filteredFilenames.json manually");
				
				try {
					var json = File.ReadAllText(_filelistJsonPath);
					var files = JsonConvert.DeserializeObject<PakFiles>(json);
					List<string>? filesOfThePak = files?.Files.First().Value;
					if(filesOfThePak == null)
						throw new NullReferenceException("filesOfThePak is null. Probably, something is wrong with your Regex.");
					var filteredFiles = filesOfThePak.Where(file => Regex.IsMatch(file, _languagesRegex));
					File.WriteAllText(_filteredFilelistJsonPath, JsonConvert.SerializeObject(filteredFiles));
				}
				catch (Exception ex) {
					throw new Exception("Error while creating filteredFilenames.json", ex);
				}
				finally {
					if(File.Exists(_filelistJsonPath))
						File.Delete(_filelistJsonPath);
				}
			}

			static public void UnpackBin()
			{
				if(!File.Exists(Path.Combine(_workingDir,_toolPath)))
					throw new FileNotFoundException("HGPakTool not found.");
						
				toolStartInfo.Arguments = $"-j {_filteredFilelistJsonPath} -U {_pakPath}";
				var process = Process.Start(toolStartInfo)!;
				process.WaitForExit();
			}
		
		}

		public class PakFiles
		{
			public Dictionary<string, List<string>> Files { get; set; }
		}
	}
}
