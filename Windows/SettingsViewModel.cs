using Avalonia.Media;
using Avalonia.PropertyGrid.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using NMS_EnglishAlienWordsMod_Avalonia.Logic;
using PropertyModels.Collections;
using PropertyModels.ComponentModel;
using PropertyModels.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text.RegularExpressions;


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
		public class ValidateRegexAttribute : ValidationAttribute
		{
			protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
			{
				if (value is string pattern)
				{
					try
					{
						_ = new Regex(pattern);
						return ValidationResult.Success;
					}
					catch (ArgumentException ex)
					{
						return new ValidationResult($"Invalid regex pattern: {ex.Message}");
					}
				}
				else
				{
					return new ValidationResult("Error while checking what error to show (wrong type for regex validator)"); //it shoudn't happen since only strings should have ValidateRegex attribute
				}
			}
		}

		public class ValidateRegexCaptureGroupAttribute : ValidationAttribute
		{
			private readonly string _regexPropertyName;
			public ValidateRegexCaptureGroupAttribute(string regexPropertyName)
			{
				_regexPropertyName = regexPropertyName;
			}
			protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
			{
				var instance = validationContext.ObjectInstance as SettingsObject;
				var regexPattern = instance?.GetType().GetProperty(_regexPropertyName)?.GetValue(instance) as string;
				if (regexPattern == null)
				{
					return new ValidationResult($"Could not find regex pattern property '{_regexPropertyName}'.");
				}
				if (value is int groupNumber)
				{
					try
					{
						var regex = new Regex(regexPattern);
						var match = regex.Match(string.Empty); // Use an empty string to get the group count
						if (groupNumber < 0 || groupNumber > match.Groups.Count - 1)
						{
							return new ValidationResult($"Capture group number must be between 0 and {match.Groups.Count - 1} for the regex pattern.");
						}
						return ValidationResult.Success;
					}
					catch (ArgumentException ex)
					{
						return new ValidationResult($"Invalid regex pattern: {ex.Message}");
					}
				}
				else
				{
					return new ValidationResult("Error while checking what error to show (wrong type for capture group validator)"); //it shoudn't happen since only int should have ValidateRegexCaptureGroup attribute
				}
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
			[EnumDisplayName("converting mbin files to xml")]
			UnpackMbins,
			[EnumDisplayName("finishing creating the mod")]
			Never
		}

		//# Settings variables

		//## Essential
		//. don't displays in the settings window, but displays in the main window

		[Category("Essential")]
		[DisplayName("No Man's Sky Game Path")]
		[Description("Path to the No Man's Sky game directory.")]
		[PathBrowsable(PathBrowsableType.Directory)]
		[ValidateGamePath]
		public string NoMansSkyGamePath { get; set; }

		//## User mod creator settings
		//. Settings that user can change to customize the mod
		[Category("Mod creator settings")]
		[DisplayName("No Man's Sky mod folder")]
		[Description("Path to the No Man's Sky mod folder.")]
		[PathBrowsable(PathBrowsableType.Directory)]
		public string NoMansSkyModsPath
		{
			get => _noMansSkyModPath ?? Path.Combine(NoMansSkyGamePath, $"GAMEDATA\\GAMEDATAS\\MODS\\{AppGlobals.ModName}");
			set => _noMansSkyModPath = value;
		}
		private string _noMansSkyModPath;

		[Category("Mod creator settings")]
		[DisplayName("Mod filename")]
		[Description("Filename of the localization table.")]
		public string ModFileName { get; set; } = "LocTable.MXML";


		//## HGPAK tool
		//. For unpacking pak file

		[Category("HGPAK tool")]
		[DisplayName("Target Pak's Path")]
		[Description("Path to the folder, containing target pak file.")]
		public string PakTargetPath { get; set; } = "GAMEDATA\\PCBANKS\\";

		[Category("HGPAK tool")]
		[DisplayName("Target Pak's Name")]
		[Description("Name of pak file containing localization files (LANGUAGE folder).")]
		public string PakTargetName { get; set; } = "NMSARC.MetadataEtc.pak";
		[Category("HGPAK tool")]
		[DisplayName("Full Target Pak's Path")]
		[Description("Full path to the pak file containing localization files (LANGUAGE folder).")]
		[ReadOnly(true)]
		public string GetPakTargetFullPath()  =>
			 Path.Combine(NoMansSkyGamePath, PakTargetPath, PakTargetName);  
		[Category("HGPAK tool")]
		[DisplayName("Languages .mbin files Regex")]
		[Description("Regular expression for finding LANGUAGE .MBIN files in the  pak. Not case-sensitive")]  
		public string languagesRegex 
		{
			get => _languagesRegex ?? $@"LANGUAGE\/NMS_(LOC|UPDATE)\d{{1,2}}_{TargetLanguage}\.MBIN";
			set => _languagesRegex = value;
		}
		private string _languagesRegex;

		//## MBIN Compiler
		//. For converting .mbin files to .xml
		[Category("Mbin compiler")]
		[DisplayName("Clean mbin files after converting")]
		[Description("If true, deletes .mbin files after converting them to .xml files")]
		public bool CleanMbinsAfterConverting { get; set; } = true;

		[Category("Mbin compiler")]
		[DisplayName("MBIN Compiler asset name")]
		[Description("Name of the asset, containing MBIN Compiler executable for your system (in Github page → Releases → Assets).")]
		public string MbinCompilerAssetName { get; set; } = "MBINCompiler.exe";
		[Category("Mbin compiler")]
		[DisplayName("MBIN Compiler versions path")]
		[Description("Path to the folder containing mbin files to unpack (HGPAK tool output folder)")]
		public string MbinTargetDirectoryPath => Path.Combine(Environment.CurrentDirectory,"Content","EXTRACTED\\language");

		[Category("Mbin compiler")]
		[DisplayName("MBIN Compiler command template")]
		[Description("Command line template for MBIN Compiler. {TargetDirectoryPath} will be replaced with actual path to the folder containing mbin files to unpack")]
		public string MbinCompilerCommand 
		{
			get => _mbinCompilerCommand ?? "{TargetDirectoryPath} --input-format=MBIN --quiet";
			set => _mbinCompilerCommand = value;
		}
		private string _mbinCompilerCommand;


		//## xml composer
		//. For creating mod files by analyzing .xml language files

		[Category("Xml composer")]
		[DisplayName("Clean all files after converting")]
		[Description("If true, deletes all NomesEnalwomo files after creating a mod")]
		public bool CleanAllFilesAfterConverting { get; set; } = true;

		[ValidateRegex]
		[Category("Xml composer")]
		[DisplayName("Localization entry block regex")]
		[Description("Regular expression for generating localization entry blocks in xml files")]
		public string LocalizationEntryBlockRegex { get; set; } = @"<Property\s+name=""Table""\s+value=""TkLocalisationEntry""([\s\S]*?)<\/Property>";

		[ValidateRegex]
		[Category("Xml composer")]
		[DisplayName("Alien words regex")]
		[Description("Regular expression for finding alien words in English xml files.")] // species: BUI, EXP, TRA, WAR
		public string AlienWordRegex { get; set; } = @"			<Property name=""Id"" value=""(?!ATLAS_(STATION|NAME))(BUI_[A-Z]*|EXP_[A-Z]*|TRA_[A-Z]*|WAR_[A-Z]*|ATLAS_[A-Z]*)""";
		[Category("Xml composer")]
		[DisplayName("Capture group")]
		[Description("Number of capture group in Alien words regex that contains the alien word itself")]
		public int AlienWordCaptureGroupNumber { get; set; } = 1;

		[ValidateRegex]
		[Category("Xml composer")]
		[DisplayName("English words regex")]
		[Description("Regular expression for finding English translation for alien words above")]
		public string EnglishWordRegex { get; set; } = @"			<Property name=""English"" value=""([a-z])"""; //todo: English can be replaced with variable TargetLanguage, but it's actually no sense of it. What did I even create TargetLanguate for?
		[Category("Xml composer")]
		[DisplayName("Capture group")]
		[Description("Number of capture group in English words regex that contains the English word itself")]
		public int EnglishWordCaptureGroupNumber { get; set; } = 1;
		/// <summary>
		/// Template for creating .mxml file. {WordBlocks} should be replaced with <see cref="WordBlockTemplate"/>
		/// </summary>
		[Category("Xml composer")]
		[DisplayName("mxml layout start")]
		[Description("Text to wrap .mxml file")]
		public string MxmlLayoutTemplate { get; set; } = 
@"<?xml version=""1.0"" encoding=""utf-8""?>
<!--File created using MBINCompiler version (6.04.0.2)-->
<Data template=""cTkLocalisationTable"">
	<Property name=""Table"">
{WordBlocks}
	</Property>
</Data>";

		/// <summary>
		/// Template for each word block in .mxml file. {AlienWord} should be replaced with alien word, {EnglishWord} with English translation, {OtherLanguages} with <see cref="OtherLanguagesTemplate"/> repeated for each language except English
		/// </summary>
		[Category("Xml composer")]
		[DisplayName("WordBlockTemplate")]
		[Description("Template for each word block in .mxml file")]
		public string WordBlockTemplate { get; set; } =
@"		<Property name=""Table"" value=""TkLocalisationEntry"">
			<Property name=""Id"" value=""{AlienWord}"" />
			{OtherLanguages}
		</Property>";

		/// <summary>
		/// Template for each language in word block. {Language} should be replaced with language name, {EnglishWord} with English translation
		/// </summary>
		[Category("Xml composer")]
		[DisplayName("OtherLanguagesTemplate")]
		[Description("Template for each language in word block")]
		public string OtherLanguagesTemplate { get; set; } = @"			<Property name=""{Language}"" value=""{EnglishWord}"" />";


		//## Generator Settings
		//. Other settings for mod generation

		[Category("Debug")]
		[DisplayName("Stop creating mod after")]
		[Description("Stops the process after specified step.")] 
		public DebugStopPoint StopAfter { get; set; } = DebugStopPoint.Never;
		[Category("Miscellaneous")]
		[DisplayName("Target Language")]
		[Description("Language that will be taken as source of alien language words. Must be English")]  
		public string TargetLanguage  { get; set; } = "English";

		[Category("Miscellaneous")]
		[DisplayName("Languages")]
		[Description("List of languages that will be included in the mod, except English")]
		public List<string> Languages { get; set; } = new() {
			"English",
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
			"Japanese",
			"USEnglish"
		};
		
	
		
		
		
	
		
		
	}

}
