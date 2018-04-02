//----------------------------------------------------------------------------------------------
// author Leonid [Zanleo] Voitko (2018.03.13)
//----------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Core.Utils;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

using ChillyRoom.UnityEditor.iOS.Xcode;

public class TextManagerLocalizator : IPreprocessBuild, IPostprocessBuild
{
	public int callbackOrder
	{
		get { return 0; }
	}

	public void OnPreprocessBuild(BuildTarget buildTarget, string path)
	{
		if (buildTarget != BuildTarget.Android)
		{
			return;
		}
		var tm = TextManager.GetComponentOnly();
		if (tm.LanguagesForLocalizationBuild.Count == 0)
		{
			return;
		}
		string valuesPath = PathExt.Combine(Application.dataPath, "Plugins", "Android", "res");
		var sb = new StringBuilder();
		sb.AppendLine("Localization of GameName (id=\"" + GameNameId + "\")");
		bool someChanged = TryLocalizeAndroid(sb, valuesPath, tm.DefaultLanguage);

		foreach (var supportedLanguage in tm.SupportedLanguages)
		{
			someChanged = TryLocalizeAndroid(sb, valuesPath, supportedLanguage);
		}

		Debug.Log(sb.ToString());
		if (someChanged)
		{
			AssetDatabase.Refresh();
		}
	}

	private static bool TryLocalizeAndroid(
		StringBuilder sb, string valuesPath, SystemLanguage lang, bool defaultLang = false)
	{
		if (!Directory.Exists(valuesPath))
		{
			Directory.CreateDirectory(valuesPath);
		}

		var tm = TextManager.GetComponentOnly();
		string langPathFolder;
		if (!defaultLang)
		{
			string langCode = GetCode(lang);
			langCode = langCode.Substring(0, 2);
			string dirName = string.Format("values-{0}", langCode);
			langPathFolder = Path.Combine(valuesPath, dirName);
		}
		else
		{
			langPathFolder = Path.Combine(valuesPath, "values");
		}

		if (!Directory.Exists(langPathFolder))
		{
			Directory.CreateDirectory(langPathFolder);
		}

		string langFilePath = Path.Combine(langPathFolder, "strings.xml");
		string localizedName;
		bool needLocalize = tm.LanguagesForLocalizationBuild.Contains(lang);

		sb.Append("\t").Append(langPathFolder);
		if (needLocalize)
		{
			tm.Load(lang);
		}
		else
		{
			sb.Append("; Use default localization");
			tm.LoadDefault();
		}
		const string AppNameKey = "\"app_name\"";
		if (tm.HasTranslation(GameNameId))
		{
			List<string> allLines;
			if (File.Exists(langFilePath))
			{
				allLines = File.ReadAllLines(langFilePath).ToList();
				if (allLines.Any(l => l.Contains(AppNameKey)))
				{
					sb.Append(". Skip");
					sb.AppendLine();
					return false;
				}
			}
			else
			{
				allLines = new List<string>
				{
					@"<?xml version=""1.0"" encoding=""utf-8""?>",
					@"<resources>",
					@"</resources>",
				};
			}

			int indexLastRes = allLines.FindIndex(l => l.ToLower().Replace(" ", "").Contains("</resources>"));
			Assert.IsTrue(indexLastRes >= 0);
			localizedName = tm.Translate(GameNameId);
			string targStrVal = string.Format("	<string name={0}>{1}</string>", AppNameKey, localizedName);
			allLines.Insert(indexLastRes, targStrVal);
			File.WriteAllLines(langFilePath, allLines.ToArray());
			sb.AppendLine();
			return true;
		}

		sb.Append(". Skip");
		sb.AppendLine();
		return false;
	}

	public void OnPostprocessBuild(BuildTarget buildTarget, string path)
	{
		if (buildTarget != BuildTarget.iOS)
		{
			return;
		}
		var tm = TextManager.GetComponentOnly();

		var projPath = PBXProject.GetPBXProjectPath(path);
		var proj = new PBXProject();
		proj.ReadFromFile(projPath);
		
		//string target = proj.TargetGuidByName("Unity-iPhone");
		var sb = new StringBuilder();
		sb.AppendLine("Localization of GameName (id=\"" + GameNameId + "\")");

		// Default localization need for InfoPlist.strings
		string langPath;
		string localizedName;

		foreach (var supportedLanguage in tm.SupportedLanguages)
		{
			bool needLocalize = tm.LanguagesForLocalizationBuild.Contains(supportedLanguage);
			string langCode = GetCode(supportedLanguage);
			string langPathFolder = string.Format("{0}.lproj", langCode);
			string fileRelatvePath = string.Format("{0}/InfoPlist.strings", langPathFolder);
			langPath = Path.Combine(path, langPathFolder);
			Directory.CreateDirectory(langPath);
			langPath = Path.Combine(langPath, "InfoPlist.strings");

			using (var stream = File.CreateText(langPath))
			{
				sb.Append("\t").Append(langPathFolder);
				if (needLocalize)
				{
					tm.Load(supportedLanguage);
				}
				else
				{
					sb.Append("; Use default localization");
					tm.LoadDefault();
				}
				if (tm.HasTranslation(GameNameId))
				{
					localizedName = tm.Translate(GameNameId);
					stream.WriteLine("\"{0}\" = \"{1}\";", "CFBundleDisplayName", localizedName);
				}
				else
				{
					sb.Append(". Skip");
				}
				sb.AppendLine();
			}

			proj.AddLocalization("InfoPlist.strings", langCode, fileRelatvePath);
		}
		sb.AppendLine("Done");
		Debug.Log(sb.ToString());
		proj.WriteToFile(projPath);
	}
	
	private static string GetCode(SystemLanguage supportedLanguage)
	{
		if (!s_langCodes.ContainsKey(supportedLanguage))
		{
			Debug.LogError("Not found code for " + supportedLanguage + "; " + s_langCodes.Count);
		}
		return s_langCodes[supportedLanguage];
	}

	private const string GameNameId = "GameName";

	private static Dictionary<SystemLanguage, string> s_langCodes = new Dictionary<SystemLanguage, string>
	{
		{SystemLanguage.Afrikaans, "af"},
		{SystemLanguage.Arabic, "ar"},
		{SystemLanguage.Basque, "eu"},
		{SystemLanguage.Belarusian, "be"},
		{SystemLanguage.Bulgarian, "bg"},
		{SystemLanguage.Catalan, "ca"},
		{SystemLanguage.Chinese, "zh"},
		{SystemLanguage.ChineseSimplified, "zh-Hans"},
		{SystemLanguage.ChineseTraditional, "zh-Hant"},
		{SystemLanguage.Czech, "cs"},
		{SystemLanguage.Danish, "da"},
		{SystemLanguage.Dutch, "nl"},
		{SystemLanguage.English, "en"},
		{SystemLanguage.Estonian, "et"},
		{SystemLanguage.Faroese, "fo"},
		{SystemLanguage.Finnish, "fi"},
		{SystemLanguage.French, "fr"},
		{SystemLanguage.German, "de"},
		{SystemLanguage.Greek, "el"},
		{SystemLanguage.Hebrew, "he"},
		{SystemLanguage.Hungarian, "hu"},
		{SystemLanguage.Icelandic, "is"},
		{SystemLanguage.Indonesian, "id"},
		{SystemLanguage.Italian, "it"},
		{SystemLanguage.Japanese, "ja"},
		{SystemLanguage.Korean, "ko"},
		{SystemLanguage.Latvian, "lv"},
		{SystemLanguage.Lithuanian, "lt"},
		{SystemLanguage.Norwegian, "no"},
		{SystemLanguage.Portuguese, "pt"},
		{SystemLanguage.Romanian, "ro"},
		{SystemLanguage.Russian, "ru"},
		{SystemLanguage.SerboCroatian, "sr"},
		{SystemLanguage.Slovak, "sk"},
		{SystemLanguage.Slovenian, "sl"},
		{SystemLanguage.Spanish, "es"},
		{SystemLanguage.Swedish, "sv"},
		{SystemLanguage.Thai, "th"},
		{SystemLanguage.Turkish, "tr"},
		{SystemLanguage.Ukrainian, "uk"},
		{SystemLanguage.Vietnamese, "vi"},
	};
}