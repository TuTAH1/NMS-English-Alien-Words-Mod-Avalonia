using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace NMS_EnglishAlienWordsMod_Avalonia.Logic
{
	internal partial class Mod
	{
		public class XmlComposer
		{
			private static List<string> _supportedLanguages => AppGlobals.CurrentSettings.Languages;

			internal static async Task<Dictionary<string, string>> ExtractAlienWordsAsync()
			{
				Dictionary<string, string> alienToEnglishDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

				string targetDirectory = AppGlobals.CurrentSettings.MbinTargetDirectoryPath;
				if (!Directory.Exists(targetDirectory)) {
					AppGlobals.MessageBuffer.AddLine($"Directory not found: {targetDirectory}", AppGlobals.MessageBuffer.MessageType.Error);
					return alienToEnglishDictionary;
				}

				string[] mxmlFiles = Directory.GetFiles(targetDirectory, "*.mxml", SearchOption.AllDirectories);
				AppGlobals.MessageBuffer.AddLine($"Found {mxmlFiles.Length} mxml files");
				await AddProgressMessage("Extracting alien words", 0);

				int totalFiles = mxmlFiles.Length;
				if (totalFiles == 0) {
					AppGlobals.MessageBuffer.AddLine("No mxml files found for processing", AppGlobals.MessageBuffer.MessageType.Error);
					return alienToEnglishDictionary;
				}
				int processedFiles = 0;

				try {
					// Compiled regex for efficiency
					Regex alienWordRegex = new Regex(AppGlobals.CurrentSettings.AlienWordRegex);
					Regex englishWordRegex = new Regex(AppGlobals.CurrentSettings.EnglishWordRegex);

					foreach (string filePath in mxmlFiles) {
						try {
							string fileContent = await File.ReadAllTextAsync(filePath);

							// Find all localization blocks
							var blocks = new Regex(AppGlobals.CurrentSettings.LocalizationEntryBlockRegex).Matches(fileContent);

							foreach (Match block in blocks) {
								string blockContent = block.Value;

								// Search for alien word in the block
								var alienMatch = alienWordRegex.Match(blockContent);
								if (!alienMatch.Success) continue;

								// Search for English word in the block
								var englishMatch = englishWordRegex.Match(blockContent);
								if (!englishMatch.Success) continue;

								// Get values from capture groups
								string alienWord = alienMatch.Groups[AppGlobals.CurrentSettings.AlienWordCaptureGroupNumber].Value;
								string englishWord = englishMatch.Groups[AppGlobals.CurrentSettings.EnglishWordCaptureGroupNumber].Value;

								if (!string.IsNullOrEmpty(alienWord) && !string.IsNullOrEmpty(englishWord)) {
									alienToEnglishDictionary[alienWord] = englishWord;
								}
							}

							processedFiles++;
							int increment = 50 / totalFiles;
							await AddProgressMessage(null, increment);
						}
						catch (Exception ex) {
							AppGlobals.MessageBuffer.AddLine($"Error processing file {Path.GetFileName(filePath)}: {ex.Message}", AppGlobals.MessageBuffer.MessageType.Error);
						}
					}
				}
				catch (Exception ex) {
					throw new Exception("Failed to extract alien words", ex);
				}

				AppGlobals.MessageBuffer.AddLine($"Extracted {alienToEnglishDictionary.Count} alien words with translations", AppGlobals.MessageBuffer.MessageType.Good);
				return alienToEnglishDictionary;
			}
			internal static async Task<string> GenerateMxmlFileAsync(Dictionary<string, string> AlienToEnglishDictionary)
			{
				if (AlienToEnglishDictionary == null || AlienToEnglishDictionary.Count == 0) {
					AppGlobals.MessageBuffer.AddLine("No alien words found for mxml generation", AppGlobals.MessageBuffer.MessageType.Error);
					return string.Empty;
				}

				StringBuilder wordBlocksBuilder = new StringBuilder();
				int totalWords = AlienToEnglishDictionary.Count;
				int processedWords = 0;

				try {
					foreach (var pair in AlienToEnglishDictionary) {
						string alienWord = pair.Key;
						string englishWord = pair.Value;

						StringBuilder otherLanguagesBuilder = new StringBuilder();

						// Generate blocks for all languages
						foreach (string language in _supportedLanguages) {
							string languageTemplate = AppGlobals.CurrentSettings.OtherLanguagesTemplate;
							string languageBlock = languageTemplate
								.Replace("{Language}", language)
								.Replace("{EnglishWord}", englishWord);

							otherLanguagesBuilder.AppendLine(languageBlock);
						}

						// Create block for current word
						string wordBlock = AppGlobals.CurrentSettings.WordBlockTemplate
							.Replace("{AlienWord}", alienWord)
							.Replace("{EnglishWord}", englishWord)
							.Replace("{OtherLanguages}", otherLanguagesBuilder.ToString().TrimEnd());

						wordBlocksBuilder.AppendLine(wordBlock);

						processedWords++;
						int increment = totalWords > 0 ? 50 / totalWords : 0;
						await AddProgressMessage(null, increment);
					}

					// Create final mxml file using template
					string finalMxml = AppGlobals.CurrentSettings.MxmlLayoutTemplate
						.Replace("{WordBlocks}", wordBlocksBuilder.ToString().TrimEnd());

					AppGlobals.MessageBuffer.AddLine("Mxml file generation completed", AppGlobals.MessageBuffer.MessageType.Good);
					return finalMxml;
				}
				catch (Exception ex) {
					throw new Exception("Failed to generate mxml file", ex);
				}
			}
			internal static async Task SaveMxmlToFileAsync(string MxmlContent)
			{
				string filePath = Path.Combine(AppGlobals.CurrentSettings.NoMansSkyModsPath, AppGlobals.CurrentSettings.ModFileName);
				if (string.IsNullOrEmpty(MxmlContent)) {
					AppGlobals.MessageBuffer.AddLine("No content to save", AppGlobals.MessageBuffer.MessageType.Warn);
					return;
				}

				try {
					await AddProgressMessage("Saving mxml file", 6);
					await File.WriteAllTextAsync(filePath, MxmlContent);
					AppGlobals.MessageBuffer.AddLine($"File saved successfully to {filePath}", AppGlobals.MessageBuffer.MessageType.Good);
				}
				catch (Exception ex) {
					throw new Exception($"Failed to save file to {filePath}", ex);
				}

			}
		}
	}
}