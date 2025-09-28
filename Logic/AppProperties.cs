using AvaloniaDialogs.Views;
using NMS_EnglishAlienWordsMod_Avalonia.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NMS_EnglishAlienWordsMod_Avalonia.Logic
{
	public static partial class AppGlobals
	{
		public static SettingsViewModel SettingsModel = new();
		public static SettingsObject CurrentSettings => SettingsModel.Target;
		//public static State CurrentState = new();
		public static SingleActionDialog? ErrorWindow {private get; set;} = null;
		//. Showing and nullifiing error window. Don't need to be awaited.
		internal static MbinCompilerManager Mbinc = null;
		public static async Task ShowError(SingleActionDialog? ErrorDialog = null)
		{
			if (ErrorDialog!=null) ErrorWindow = ErrorDialog;
			if (ErrorWindow == null) return;
			await ErrorWindow.ShowAsync();
			ErrorWindow = null;
		}

		public static readonly List<string> HiddenSettingsWindowCategories = new()
		{
			"Essential"
		};

		public static class MessageBuffer { 
			private static string _text = "";
			private static string _newLine = "\r\n";
			private static string _standardNewLine = "\n";
			static void AddText(string text, string color = "white")
			{
				_text += $"%{{color:{color}}}{text}%";
			}

			static void AddLine(string text, string color = "white")
			{
				AddText(text, color);
				_text += _standardNewLine;
			}
			public static void Clear()
			{
				_text = "";
			}
			public static string GetAndClear() 
			{ 
				var temp = MarkdownizeNewLines(_text); 
				Clear(); 
				return temp;
			}
					
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
				_text += _standardNewLine;
			}

			public static void AddLine(Exception exception) => 
				AddLine(ExceptionToMarkdown(exception), MessageType.Error);

			private static string ExceptionToMarkdown(Exception exception)
			{
				 return $@"
<details>
<summary>{exception.Message}</summary>
	<details>
	<summary>Stack Trace</summary>
	{exception.StackTrace}
	</details>
	{(exception.InnerException != null ? $"<details><summary>Inner Exception</summary>{ExceptionToMarkdown(exception.InnerException)}</details>" : "")}
</details>
";

			}

			private static string MarkdownizeNewLines(string text) => text.Replace("\n", _newLine);
		}
	}
}
