using Avalonia.Media;
using Avalonia.PropertyGrid.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using PropertyModels.Collections;
using PropertyModels.ComponentModel;
using PropertyModels.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;


namespace NMS_EnglishAlienWordsMod_Avalonia.Windows
{
	public partial class SettingsViewModel : ReactiveObject
	{
		public SettingsObject Target { get; set; } = new SettingsObject();

	}

	public class SettingsObject : ReactiveObject
	{
		public class ValidateGamePathAttribute : ValidationAttribute
		{
			protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
			{
			if (value is string path)
			{
				var instance = validationContext.ObjectInstance as SettingsObject;
				var pakFilePath = instance.GetPakTargetFullPath();
				if (File.Exists(pakFilePath))
					return ValidationResult.Success;
				else
					if (string.IsNullOrEmpty(path)) //: it's checked after Exist() if it somehow still be the correct path. But if it don't, now we look if it's becouse it's incorrect or becouse it's empty
						return new ValidationResult("Shouldn't be empty.");
					else
						return new ValidationResult($"Invalid game path. Can't find '{pakFilePath}' file");
			}
			else
				return new ValidationResult("Error while checking what error to show (wrong type for path validator)"); //it shoudn't happen since only strings should have ValidateGamePath attribute

				 
			}
		}

		public enum DebugStopPoint
		{
			[EnumDisplayName("creating filelist.json file")]
			FilelistJson,
			[EnumDisplayName("filtering filelist.json and creating changedFilelist.json file")]
			changedFilelistJson,
			[EnumDisplayName("unpacking bin files to temp directory")]
			UnpackBin,
			[EnumDisplayName("finishing creating the mod")]
			Never
		}

		[Category("Essential")]
		[DisplayName("No Man's Sky Game Path")]
		[Description("Path to the No Man's Sky game directory.")]
		[PathBrowsable(PathBrowsableType.Directory)]
		[ValidateGamePath]

		public string NoMansSkyGamePath { get; set; }

		[Category("GeneratorSettings")]
		[DisplayName("Target Pak's Path")]
		[Description("Path to the folder, containing target pak file.")]
		public string PakTargetPath { get; set; } = "GAMEDATA\\PCBANKS\\";

		[Category("GeneratorSettings")]
		[DisplayName("Target Pak's Name")]
		[Description("Name of pak file containing localization files (LANGUAGE folder).")]
		public string PakTargetName { get; set; } = "NMSARC.MetadataEtc.pak";

		[Category("GeneratorSettings")]
		[DisplayName("MBIN Compiler asset name")]
		[Description("Name of the asset, containing MBIN Compiler executable for your system (in Github page → Releases → Assets).")]
		public string GetPakTargetFullPath()  =>
			 Path.Combine(NoMansSkyGamePath, PakTargetPath, PakTargetName);  
		
		public string MbinCompilerAssetName { get; set; } = "MBINCompiler.exe";
		[Category("GeneratorSettings")]
		[DisplayName("LanguagesBinFilesRegex")]
		[Description("Regex for finding LANGUAGE .BIN files in the  pak.")]  
		public string languagesRegex { get; set; }  = @"LANGUAGE\/NMS_(LOC|UPDATE)\d{1,2}_ENGLISH\.BIN";
		
		[Category("GeneratorSettings")]
		[DisplayName("Target Language")]
		[Description("Language that will be taken as source of alien language words. Must be English")]  
		public string TargetLanguage  { get; set; } = "English";

		[Category("GeneratorSettings")]
		[DisplayName("Languages")]
		[Description("List of languages that will be included in the mod, except English")]
		public List<string> Languages { get; set; } = new() {
			"French",
			"Italian",
			"German",
			"Spanish",
			"Russian",
			"Polish",
			"Dutch",
			"Portuguese",
			"LatinAmericanSpanish",
			"BrazilianPortuguese",
			"SimplifiedChinese",
			"TraditionalChinese",
			"TencentChinese",
			"Korean",
			"Japanese"
		};
		
		[Category("GeneratorSettings")]
		[DisplayName("Stop creating mod after")]
		[Description("Stops the process after specified step.")] 
		public DebugStopPoint StopAfter { get; set; } = DebugStopPoint.Never;
		
		
		
	
		
		
	}

}
