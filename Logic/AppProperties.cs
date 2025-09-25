using NMS_EnglishAlienWordsMod_Avalonia.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMS_EnglishAlienWordsMod_Avalonia.Logic
{
	public static class AppProperties
	{
		public static SettingsViewModel SettingsModel = new();
        public static SettingsObject CurrentSettings => SettingsModel.Target;
	}
}
