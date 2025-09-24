using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using PropertyModels.ComponentModel;
using PropertyModels.ComponentModel.DataAnnotations;
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
		[DisplayName("No Man's Sky Game Path")]
		public string NoMansSkyGamePath { get; set; } //TODO: вынести в текстовое поле на главную форму

		[DisplayName("Target Pak's Path")]
		[Description("Path to the folder, containing target pak file.")]
		public string PakTargetPath { get; set; } = "GAMEDATA\\PCBANKS\\";

		[DisplayName("Target Pak's Name")]
		[Description("Name of pak file containing localization files (LANGUAGE folder).")]
		public string PakTargetName { get; set; } = "NMSARC.MetadataEtc.pak";

		[DisplayName("MBIN Compiler asset name")]
        [Description("Name of the asset, containing MBIN Compiler executable for your system (in Github page → Releases → Assets).")]
		public string MbinCompilerAssetName { get; set; } = "MBINCompiler.exe";
	}

}
