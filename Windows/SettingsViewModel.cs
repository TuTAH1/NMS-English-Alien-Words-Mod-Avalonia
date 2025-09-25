using Avalonia.Media;
using Avalonia.PropertyGrid.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using PropertyModels.ComponentModel;
using PropertyModels.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace NMS_EnglishAlienWordsMod_Avalonia.Windows
{
	public partial class SettingsViewModel : ReactiveObject
	{
		public SettingsObject Target { get; set; } = new SettingsObject();

	}

	public class SettingsObject : ReactiveObject
	{
		[Category("Essential")]
		[DisplayName("No Man's Sky Game Path")]
		[Description("Path to the No Man's Sky game directory.")]
		[PathBrowsable(PathBrowsableType.Directory)]

		public string NoMansSkyGamePath { get; set; } //TODO: вынести в текстовое поле на главную форму

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
		public string MbinCompilerAssetName { get; set; } = "MBINCompiler.exe";

		[Category("GeneratorSettings")]
		[DisplayName("Languages")]
        [Description("List of languages that will be included in the mod")]
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
