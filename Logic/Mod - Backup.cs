//using Avalonia.Controls;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using Tmds.DBus.Protocol;

//namespace NMS_EnglishAlienWordsMod_Avalonia.Logic
//{
//    internal class Mod
//    {
//	    public string XMLExtension = ".BXML";

//		public async Task Convert(string FolderPath, string MBINCPath, bool MBIN, string[] LangNames)
//		{
//			int procCountBeg = Process.GetProcessesByName("MBINCompiler.exe").Length; //:How many MCompilers running before launch
//			int procCount = 0;
//			string sourceEXT = MBIN ? ".MBIN" : XMLExtension;
//			string targetEXT = !MBIN ? ".MBIN" : XMLExtension;
//			bool noErrors = true;
//		progressBar.Initialize();
//			AddDebugLine($"Launching MBINC for converting {sourceEXT} to {targetEXT}");
//			progressBar.Maximum = (FileNames.Length * LangNames.Length+2);
//			//:files that can't be unpacked and should be skipped while checking convertion ending
//			List<string> exceptionFilesData = new List<string>(LangNames.Length*FileNames.Length);

//			foreach (var locFile in FileNames)
//			{
//				foreach (var lang in LangNames)
//				{
//					if (ExceptLangLames.Contains(lang)) continue;
//					string filePath = FolderPath + locFile + lang.ToUpper() + sourceEXT;
//					string filePathQuotes = $"\"{filePath}\"";
//					if (File.Exists(FolderPath + locFile + lang.ToUpper() + targetEXT))
//					{
//						AddDebugLine($"{filePathQuotes} is already unpacked", MessageType.Warning);
//						exceptionFilesData.Add(locFile+lang);
//						progressBar.Increment(1);
//					}
//					else
//					{
//						try
//						{
//							if (File.Exists(filePath))
//							{
//								var p = new Process(); //Process.Start();
//								p.StartInfo.FileName = Settings.Default.MBINCompiler_Path;
//								p.StartInfo.Arguments = "convert -q -y " + filePathQuotes; //:-f for skip errors -Q for no log;
//								p.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
//								p.Start();
//								procCount++;
//							}
//							else
//							{
//								AddDebugLine($"Can't find file {filePathQuotes}", MessageType.Error);
//								exceptionFilesData.Add(locFile+lang);
//								progressBar.Increment(1);
//								noErrors = false;
//							}
//						}
//						catch (Exception e)
//					{
//							AddDebugLine($"Exception: {e.Message}", MessageType.Error);
//							exceptionFilesData.Add(locFile+lang);
//							progressBar.Increment(1);
//							noErrors = false;
//						}
//					}

//				}
//			}

//			progressBar.Increment(2);
//			//AddDebugLine("Waiting for converting");

//			//foreach (var locFile in FileNames)
//			//{
//			//	foreach (var lang in LangNames)
//			//	{
//			//		if (ExceptLangLames.Contains(lang)) continue;
//			//		if (exceptionFilesData.Contains(locFile+lang)) continue;
//			//		while (!File.Exists(Settings.Default.OriginalFilesPath + "\\"+ locFile + lang+".EXML"))
//			//		{
//			//			Application.DoEvents();
//			//			Thread.Sleep(300); //TODO: Find a better func for pause
//			//			//Timer timer = new Timer();
//			//		}
					
//			//	}
//			//}

//			AddDebugLine("Waiting for the conversion completion");
//			//progressBar.Style = ProgressBarStyle.Marquee;
//			while (procCount > procCountBeg)
//			{
//				Application.DoEvents();
//				Thread.Sleep(300); //TODO: Find a better func for pause
//				var newProcCount = Process.GetProcessesByName("MBINCompiler").Length - procCountBeg;
//				if (newProcCount != procCount)
//				{
//					progressBar.Increment(procCount-newProcCount);
//					procCount = newProcCount;
//				}
//			}

//			AddDebugLine("Conversion done", MessageType.Good);

//			progressBar.Visible = false;
//			CheckPaths(InputFolder:true);
//		}

//		public void CreateLocalisationFilesFast(string InputFolderPath, string OutputFolderPath, string[] LangNames)
//		{
//			//:There's will be optimized but a bit more risky version of {CreateLocalisationFiles()} if needed
//		}

//		public async void CreateLocalisationFiles(string InputFolderPath, string OutputFolderPath, string[] LangNames)
//		{
//			progressBar.Initialize();
//			string[] acceptedLangNames = new string[LangNames.Length - ExceptLangLames.Length-1];
//			{
//				int i = 0;
//				foreach (var lang in LangNames)
//				{
//					if (!(ExceptLangLames.Contains(lang) || lang == LangNames[0]))
//				{
//						acceptedLangNames[i] = lang;
//						i++;
//					}
//				}
//			}
//			progressBar.Maximum = (int)(acceptedLangNames.Length*FileNames.Length*3);

//			for (int f = 0; f < FileNames.Length; f++)//! that can be done in parrallel mode
//			{
//				string firstWord = FirstWords[f]; //: in order not to slow down the program by constantly accessing the array
//				string lastWord = LastWords[f];
//				List<Word> words = new List<Word>(WordCountApprox[f]); //! Word can be simplified to string by not cheking ID. Would cause UB (unlikely) instead of error
//				StreamReader fileEnglish = new StreamReader(InputFolderPath + FileNames[f] + LangNames[0].ToUpper() + XMLExtension,Encoding.UTF8);
//				AddDebugLine("Searching for WORD in English file");

//				string lineEnglish = null;
//				string WORD;

//				if (firstWord != "-sof")
//				{
//					while (!fileEnglish.EndOfStream)
//					{
//						lineEnglish = fileEnglish.ReadLine();
//						if (lineEnglish.Contains(firstWord)) break;
//					}

//					string id = firstWord;

//					lineEnglish = fileEnglish.ReadLine(); //! error if next line to ID is not an english property
//					if (lineEnglish.Contains("<Property name=\"English\"")) //! Code duplication
//					{
//						lineEnglish = fileEnglish.ReadLine();
//						WORD = lineEnglish.Slice("<Property name=\"Value\" value=\"", "\" />");
//						words.Add(new Word(id, WORD));
//					}
//					else
//					{
//						throw new Exception($"«<Property name=\"English\"» expected after id {id}, but found {lineEnglish}");
//					}
//				}

//				progressBar.Increment(1);
//				AddDebugLine("Writting English words in memory");
//				while (true)
//				{
//					lineEnglish = fileEnglish.ReadLine();
//					if (lineEnglish == null) 
//						if (lastWord == "-eof") break;
//						else throw new EndOfStreamException($"English file {FileNames[f]} ended before it should"); //TODO: обработать исключение

//					string id = lineEnglish.Slice(" <Property name=\"Id\" value=\"", "\" />");
//					if (id != null)
//					{
//						lineEnglish = fileEnglish.ReadLine();
//						if (lineEnglish.Contains("<Property name=\"English\""))
//						{
//							lineEnglish = fileEnglish.ReadLine();
//							WORD = lineEnglish.Slice("<Property name=\"Value\" value=\"", "\" />");
//							words.Add(new Word(id, WORD));
//							if (id == lastWord) break;
//						}
//						else
//						{
//							throw new Exception($"«<Property name=\"English\"» expected after id {id}, but found {lineEnglish}");
//						}
//					}
//				}
				
//				fileEnglish.Close(); 

//				progressBar.Increment(2);

//				StreamReader[] filesLang = new StreamReader[acceptedLangNames.Length];
//				StreamWriter[] newFilesLangs = new StreamWriter[acceptedLangNames.Length];
//				for (int l = 0; l < acceptedLangNames.Length; l++)
//				{
//					filesLang[l] = new StreamReader(InputFolderPath + FileNames[f] + acceptedLangNames[l].ToUpper() + XMLExtension,Encoding.UTF8); //:Opening original files
//					newFilesLangs[l] = new StreamWriter(OutputFolderPath + FileNames[f] + acceptedLangNames[l].ToUpper() + XMLExtension,false,Encoding.UTF8); //:Creating mod files
//					AddDebugLine($"Searching for WORD in {FileNames[f]+acceptedLangNames[l]} file");

//					string line = null;
//					if (firstWord != "-sof")
//					{
//						do //:Copying localisation from original file
//						{
//							line = filesLang[l].ReadLine();
//							if (line == null) throw new EndOfStreamException($"File {FileNames[f]+acceptedLangNames[l]} ended before it should"); //TODO: обработать исключение
//							newFilesLangs[l].WriteLine(line);
//						} while (!line.Contains(firstWord));
//					}

//					progressBar.Increment(1);

//					AddDebugLine($"Replacing words localisation in {FileNames[f]+acceptedLangNames[l]}");

//					string wordID = firstWord;
//					bool end = false;
//					int w = 0;
//					//newFilesLangs[l].Close(); //\ for Debug
//					do
//					{
//						//newFilesLangs[l] = new StreamWriter(OutputFolderPath + FileNames[f] + acceptedLangNames[l].ToUpper() + ".EXML",true,Encoding.UTF8); //\ for Debug
//						string id;
//						id = line.Slice("<Property name=\"Id\" value=\"", "\" />"); //!loop can be simplified to goind to <Property name="{acceptedLangNames[i]}" line and skipping 53 lines. Will cause UB if languages number changes. UB can be fixed by gathering langnum while reading first TkLocalisationEntry

//						if (id != null)
//						{
//							if (end) break;
//							else
//							if (id == lastWord) end = true;
//							else
//							if (id == words[w].ID)
//							{
//								//newFilesLangs[l].Close(); //\ for Debug
//								while (true) //TODO: fix [UB if langname will not be found (unlikely)]
//								{
//									//newFilesLangs[l] = new StreamWriter(OutputFolderPath + FileNames[f] + acceptedLangNames[l].ToUpper() + ".EXML",true,Encoding.UTF8); //\ for Debug
//									line = filesLang[l].ReadLine(); //!can be optimizated by skipping 3 lines. UB improbable.
//									if (line.Contains($"<Property name=\"{acceptedLangNames[l]}\""))
//									{
//										newFilesLangs[l].WriteLine(line);
//										filesLang[l].ReadLine();
//										newFilesLangs[l].WriteLine($"        <Property name=\"Value\" value=\"{words[w++].value}\" />"); //:Putting English WORD in new localisation file
//										break;
//									} 
//									else newFilesLangs[l].WriteLine(line);
//								//	newFilesLangs[l].Close(); //\ for Debug
//								}
//							}
//							else
//							{
//								throw new Exception($"id {words[w].ID} expected, but {id} found"); //TODO:обработать исключиение
//							}
//						}

//						line = filesLang[l].ReadLine(); //reading next line
//						if (line==null)
//							if (lastWord == "-eof") break;
//							else throw new EndOfStreamException($"ile {FileNames[f] + acceptedLangNames[l] + XMLExtension} ended before it should"); //TODO: обработать исключение
//						newFilesLangs[l].WriteLine(line);
//					//	newFilesLangs[l].Close(); //\ for Debug

//					} while (true);
//					progressBar.Increment(1);

//					while (line!=null) //:Дозапись
//					{
//						line = filesLang[l].ReadLine();
//						newFilesLangs[l].WriteLine(line);
//					}
//					filesLang[l].Close();
//					newFilesLangs[l].Close();
//					progressBar.Increment(1);

//				}
//			}

//			AddDebugLine("Localisation files done",MessageType.Good);
//			progressBar.Visible = false;
//			AddDebugLine("Converting localisation files to MBIN");

//			foreach (var fileName in FileNames) //:deleting .Mbin if it exist just in case
//			{
//				foreach (var lang in acceptedLangNames)
//				{
//					var path = OutputFolderPath + fileName + lang + ".MBIN";
//					try
//					{
//						if (File.Exists(path))
//						{
//							File.Delete(path);
//							AddDebugLine($"{path} already exist. It have been deleted", MessageType.Error);
//						}
//					}
//					catch (Exception e)
//					{
//						AddDebugLine("Error:"+e.Message,MessageType.Error);
//					}
//				}
//			}

//			await Convert(OutputFolderPath, Settings.Default.MBINCompiler_Path, false,acceptedLangNames);
//			AddDebugLine($"Deleting {XMLExtension} files");

//			foreach (var fileName in FileNames)
//			{
//				foreach (var lang in LangNames)
//				{
//					string path = OutputFolderPath + fileName + lang;
//					try
//					{
//						if (File.Exists(path+ ".MBIN"))
//							File.Delete(OutputFolderPath + fileName + lang + XMLExtension);
//						else 
//							AddDebugLine($"File {path} haven't been converted", MessageType.Error);
//					}
//					catch (Exception e)
//					{
//						AddDebugLine("Error:"+e.Message);
//					}
//				}
//			}

//			AddDebugLine($"{XMLExtension} files deleted");
//		}

//		private void CheckMbincErrors(bool ShowMessageBox = true) //: Перед запуском mbinc записывать дату-время, а после завершения искать только файлы позже этой даты по паттерну MBINCompiler.*.log
//		{
//			string mbincFolder = Settings.Default.MBINCompiler_Path.Slice(0, "\\");
//			string[] logs = Directory.GetFiles(mbincFolder,"MBINCompiler.*.log");
//			foreach (var log in logs)
//			{
//				StreamReader reader = new StreamReader(log);
//				string line, errors = "", errorFilePath = null;
//				do
//				{
//					line = reader.ReadLine();
//					if (line.StartsWith("[ARGS]:")&&errorFilePath==null) errorFilePath = line;
//					if (line.StartsWith("[ERROR]:"))
//					{
//						do
//						{
//							errors += "\n" + line;
//							line = reader.ReadLine();
//						} while (line!=null && line.StartsWith("["));
//					}
//				} while (line != null);

//				if (errors != "")
//				{
//					MessageBox.Show($"Convertation of file {errorFilePath} finished with errors: {errors}");
//				}
//			}
//		}
//	}
//}
