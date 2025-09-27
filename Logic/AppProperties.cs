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
		//public static State CurrentState = new();

		public static class CurrentState
		{
			public static class MessageBuffer { 
				private static string _text = "";
				private static string _newLine = "<br/>";
				static void AddText(string text, string color = "white")
				{
					_text += $"%{{color:{color}}}{text}%";
				}

				static void AddLine(string text, string color = "white")
				{
					AddText(text, color);
					_text += _newLine;
				}
				public static string Get() => _text;
				public static void Clear()
				{
					_text = "";
				}
				public static string GetAndClear() { var temp = _text; Clear(); return temp; }
					
				public enum MessageType { Info, Warn, Error, Good }
				public static void AddText(string message, MessageType type = MessageType.Info)
				{
					if (type == MessageType.Info)
						_text += message;
					else
						AddText(message, type switch { 
							MessageType.Warn => "orange", 
							MessageType.Error => "red",
							MessageType.Good => "green",
							_ => "white" } );
				}
				public static void AddLine(string message, MessageType type = MessageType.Info)
				{
					AddText(message, type);
					_text += _newLine;
				}
			
			}
		}

	}
}
