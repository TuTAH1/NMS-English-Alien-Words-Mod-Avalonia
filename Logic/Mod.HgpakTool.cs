using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NMS_EnglishAlienWordsMod_Avalonia.Logic
{
	

	internal partial class Mod
	{
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
}
}
