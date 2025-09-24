using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace NMS_EnglishAlienWordsMod_Avalonia.Logic
{
	internal class Mod
	{
		string pakFile = "MetadataEtc.pak";
		string languagesRegex = @"LANGUAGE\/NMS_(LOC|UPDATE)\d{1,2}_ENGLISH\.BIN";
		private string[] _languagesList;
		public string[] LanguagesList => SetLanguagesList(); //. List of Property names of languages
		string languagesListPath = "Content/LanguagesList.txt";

		private string[] GetLanguagesList() => File.ReadAllLines(languagesListPath);
		private string[] SetLanguagesList() => _languagesList??= GetLanguagesList();



	}
}
