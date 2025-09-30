using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NMS_EnglishAlienWordsMod_Avalonia.Logic
{
	

	internal partial class Mod
	{
		/// <summary>
		/// Wrapper for MBINCompiler functionality
		/// unpaks all .mbin files in the specified target directory to .xml files... that's all. I don't know why did I created this class just for 1 func
		/// </summary>
		private static class MbinCompiler
		{
			public static string TargetDirectoryPath => AppGlobals.CurrentSettings.MbinTargetDirectoryPath;
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
					toolStartInfo.Arguments = AppGlobals.CurrentSettings.MbinCompilerCommand; //TODO: can be moved to settings, but it requires making a method for replacing keywords with settings variables 
					var process = Process.Start(toolStartInfo)!;
			
					await LogProcessAsync(process, 30);

					
					if (!AppGlobals.CurrentSettings.CleanMbinsAfterConverting) return;
					//: Clean up .mbin files after unpacking
					var mbinFiles = Directory.GetFiles(TargetDirectoryPath, "*.mbin", SearchOption.AllDirectories);
					foreach (var file in mbinFiles)
						File.Delete(file);
					
				});
			}
		}
}
}
