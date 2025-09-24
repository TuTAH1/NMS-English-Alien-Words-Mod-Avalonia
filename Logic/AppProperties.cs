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
		private static SettingsViewModel _currentSettings = new();
        public static SettingsObject Settings => _currentSettings.Target;
	}
}
