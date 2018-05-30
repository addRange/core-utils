//----------------------------------------------------------------------------------------------
// author Leonid [Zanleo] Voitko
//----------------------------------------------------------------------------------------------

//#undef UNITY_EDITOR
//#define UNITY_IPHONE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Core.Utils;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Assert = Core.Utils.Assert;
#if UNITY_EDITOR
using System.Xml;
using UnityEditor;
using ExcelLibrary.SpreadSheet;
#endif

public class TextManager : SingletonGameObject<TextManager>
{
	public Action EventLanguageChanged = null;

	protected override void Init()
	{
		base.Init();
		CurrentLanguage = SystemLanguage.Unknown;
		CurrentLanguageChanged = false;
		// TODO: later load saved
		if (LoadSaved())
		{
			return;
		}

		LoadSystemLanguage();
	}

	[ContextMenu("LoadSystemLanguage")]
	private void LoadSystemLanguage()
	{
		SystemLanguage targetLanguage = SystemLanguage.Unknown;
		if (m_useOnlyDefaults ||
			 Application.platform == RuntimePlatform.WindowsEditor ||
			 Application.platform == RuntimePlatform.OSXEditor ||
			 Application.platform == RuntimePlatform.LinuxEditor)
		{
			targetLanguage = DefaultLanguage;
		}
		else
		{
			targetLanguage = Application.systemLanguage;
		}
		Load(targetLanguage);
	}

	[ContextMenu("LoadDefault")]
	public void LoadDefault()
	{
		Load(DefaultLanguage);
	}
	
	public void LoadAsCurrent(SystemLanguage forCurrentLanguage)
	{
		Load(forCurrentLanguage);
	}

	public void Load(SystemLanguage currentLanguage)
	{
		SystemLanguage corrected = TryGetCorrectedLanguage(currentLanguage);
		if (corrected == SystemLanguage.Unknown)
		{
			corrected = DefaultLanguage;
			currentLanguage = DefaultLanguage;
		}
		var textAsset = Resources.Load<TextAsset>(m_pathToLanguages + corrected.ToString());
		if (textAsset == null)
		{
			Debug.LogError("Wrong loaded language " + currentLanguage);
			return;
		}

		if (CurrentLanguage == currentLanguage)
		{
			return;
		}
		CurrentLanguage = currentLanguage;
		if (m_dictionary.Count > 0)
		{
			m_dictionary.Clear();
		}
		var serializer = new XmlSerializer(typeof(XmlSerializableDictionary));
		using (var reader = new StringReader(textAsset.text))
		{
			m_dictionary = serializer.Deserialize(reader) as XmlSerializableDictionary;
		}
		//Debug.Log(m_dictionary.Aggregate(string.Empty, (current, kvp) => current + (kvp.Key + " = " + kvp.Value + "\r\n")));
		Resources.UnloadAsset(textAsset);
		EventLanguageChanged.SafeInvoke();
	}

	private SystemLanguage TryGetCorrectedLanguage(SystemLanguage forCorrection)
	{
		if (m_supportedLanguages.Contains(forCorrection))
		{
			return forCorrection;
		}
		if (m_replaceLanguages == null)
		{
			return SystemLanguage.Unknown;
		}

		return m_replaceLanguages.TryGetCorrectedLanguage(forCorrection);
	}

	private bool LoadSaved()
	{
		string savedValue = PlayerPrefs.GetString(SavedLanguageKey, string.Empty);
		if (string.IsNullOrEmpty(savedValue))
		{
			return false;
		}

		SystemLanguage targetLang = SystemLanguage.Unknown;
		try
		{
			targetLang = (SystemLanguage) Enum.Parse(typeof(SystemLanguage), savedValue);
		}
		catch
		{
			return false;
		}

		if (targetLang == SystemLanguage.Unknown)
		{
			return false;
		}

		LoadAsCurrent(targetLang);
		return true;
	}

	public void SaveCurrentLanguage()
	{
		CurrentLanguageChanged = false;
		PlayerPrefs.SetString(SavedLanguageKey, m_currentLanguage.ToString());
	}

	public bool HasTranslation(string key) { return m_dictionary.ContainsKey(key); }

	/// <summary>
	/// Translate key
	/// </summary>
	/// <param name="key"></param>
	/// <returns></returns>
	public string Translate(string key)
	{
		string res;
		if (m_dictionary.TryGetValue(key, out res))
		{
			return res;
		}

		return key;
	}

	[Conditional("UNITY_EDITOR")]
	private void OnValidate()
	{
		EditorCheckSupportedLanguages();
		// Axiom)
		Assert.IsTrue(m_supportedLanguages.Contains(SystemLanguage.English),
						  "SupportedLanguages not contain main language - english");
		if (!m_supportedLanguages.Contains(DefaultLanguage))
		{
			m_defaultLanguage = SystemLanguage.English;
			m_supportedLanguages.Add(DefaultLanguage);
			CheckOrCreateFileLocalization();
		}
	}

	private void CheckOrCreateFileLocalization()
	{
		var path = m_pathToLanguages + DefaultLanguage + ".txt";
		var text = Resources.Load<TextAsset>(path);
		if (text != null)
		{
			return;
		}
		var fullPath = Application.dataPath + "/Resources/" + path;
		if (File.Exists(fullPath))
		{
			return;
		}
		var dirPath = Path.GetDirectoryName(fullPath);
		Directory.CreateDirectory(dirPath);
		using (var writer = File.CreateText(fullPath))
		{
			var serializer = new XmlSerializer(typeof(XmlSerializableDictionary));
			var testData = new XmlSerializableDictionary();
#if UNITY_EDITOR
			testData.EditorAddToSection("Main", "GameName", Application.productName);
#endif
			serializer.Serialize(writer.BaseStream, testData);
		}
		Debug.Log("Created new text file for localization");
#if UNITY_EDITOR
		UnityEditor.AssetDatabase.Refresh();
#endif
	}

	public string GetUsedCharacters(bool withAllNumbers = false)
	{
		HashSet<char> usedCharacters = new HashSet<char>();
		if (withAllNumbers)
		{
			var numbers = new[] {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9'};
			for (var i = 0; i < numbers.Length; i++)
			{
				var number = numbers[i];
				usedCharacters.Add(number);
			}
		}
		foreach (KeyValuePair<string, string> keyVal in m_dictionary)
		{
			for (var i = 0; i < keyVal.Value.Length; i++)
			{
				var curChar = keyVal.Value[i];
				if (!usedCharacters.Contains(curChar))
				{
					usedCharacters.Add(curChar);
				}
			}
		}
		
		return new string(usedCharacters.ToArray());
	}

	[Conditional("UNITY_EDITOR")]
	public void EditorCheckSupportedLanguages()
	{
		var textAssets = Resources.LoadAll<TextAsset>(m_pathToLanguages).ToList();
		foreach (var value in Enum.GetValues(typeof(SystemLanguage)))
		{
			var val = (SystemLanguage) value;
			if (textAssets.Find(ta => ta.name == val.ToString()) != null)
			{
				if (!m_supportedLanguages.Contains(val))
				{
					m_supportedLanguages.Add(val);
					Debug.Log("Textmanager.Added to supported " + val, this);
#if UNITY_EDITOR
					UnityEditor.EditorUtility.SetDirty(transform.root.gameObject);
#endif
				}
			}
			else
			{
				if (m_supportedLanguages.Contains(val))
				{
					m_supportedLanguages.Remove(val);
					Debug.Log("Textmanager.Deleted from supported " + val, this);
#if UNITY_EDITOR
					UnityEditor.EditorUtility.SetDirty(transform.root.gameObject);
#endif
				}
			}
		}
#if UNITY_EDITOR
		UnityEditor.EditorUtility.SetDirty(gameObject);
#endif
	}
	
#if UNITY_EDITOR
	[ContextMenu("ToExcel")]
	private void ToExcel()
	{
		string path = GetPathToExcelFile("Import");
		if (File.Exists(path))
		{
			File.Delete(path);
		}
		Workbook workbook = new Workbook();
		Worksheet worksheet = new Worksheet("Localization");
		Load(DefaultLanguage);
		int row = 0;
		worksheet.Cells[row, 1] = new Cell("ID");
		worksheet.Cells[row, 2] = new Cell("English");

		row++;
		foreach (var keyPair in m_dictionary)
		{
			worksheet.Cells[row, 1] = new Cell(keyPair.Key);
			worksheet.Cells[row, 2] = new Cell(keyPair.Value);
			row++;
		}
		workbook.Worksheets.Add(worksheet);
		workbook.Save(path);

		string assetPath = path.Substring(Application.dataPath.Length + 1 - "Assets/".Length);
		AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
		var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
		Debug.Log("Import done to " + path, asset);
	}

	[ContextMenu("FromExcel")]
	private void FromExcel()
	{
		string path = GetPathToExcelFile("Export");
		if (!File.Exists(path))
		{
			Debug.LogWarning("File not found " + path);
			return;
		}
		LoadDefault();
		var workbook = Workbook.Load(path);
		var worksheet = workbook.Worksheets[0];
		int row = 0;
		string key = worksheet.Cells[row, 1].StringValue;
		int col = 2;
		string value = worksheet.Cells[row, col].StringValue;
		var langLocalizations = new Dictionary<string, XmlSerializableDictionary>();
		while (!string.IsNullOrEmpty(value))
		{
			langLocalizations.Add(value, new XmlSerializableDictionary());
			col++;
			value = worksheet.Cells[row, col].StringValue;
		}

		row++;
		bool oneNotEmpty = true;
		while (oneNotEmpty)
		{
			col = 1;
			key = worksheet.Cells[row, 1].StringValue;
			foreach (var langLocalization in langLocalizations)
			{
				col++;
				value = worksheet.Cells[row, col].StringValue;
				if (string.IsNullOrEmpty(value))
				{
					oneNotEmpty = false;
					continue;
				}

				string section = m_dictionary.EditorGetSectionName(key);
				langLocalization.Value.EditorAddToSection(section, key, value);
			}
			//oneNotEmpty = false;
			row++;
		}

		foreach (var langLocalization in langLocalizations)
		{
			SaveLocalization(langLocalization.Key, langLocalization.Value);
		}
	}

	private void SaveLocalization(string lang, XmlSerializableDictionary langLocalization)
	{
		var serializer = new XmlSerializer(typeof(XmlSerializableDictionary));
		string path = Application.dataPath + "/" + "Resources" + "/" + m_pathToLanguages + lang + ".txt";
		using (var file = new FileStream(path, FileMode.Create))
		{
			var settings = new XmlWriterSettings();
			settings.IndentChars = "\t";
			settings.Indent = true;
			using (var writer = XmlTextWriter.Create(file, settings))
			{
				serializer.Serialize(writer, langLocalization);
				writer.Close();
			}
		}
		string assetPath = path.Substring(Application.dataPath.Length + 1 - "Assets/".Length);
		AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
		Debug.Log("Lang " + lang + " saved to path " + path);
	}

	private string GetPathToExcelFile(string name)
	{
		string path = Application.dataPath;
		path = Path.Combine(path, "Resources");
		path = Path.Combine(path, PathToPrefabs);
		path = Path.Combine(path, name + ".xls");
		return path;
	}

	[ContextMenu("Fill languagesForLocalizationBuild from supported")]
	private void FillFromSupported()
	{
		m_languagesForLocalizationBuild.AddRange(
			m_supportedLanguages.FindAll(s => !m_languagesForLocalizationBuild.Contains(s)));
	}
#endif

	public SystemLanguage CurrentLanguage
	{
		get { return m_currentLanguage; }
		private set
		{
			CurrentLanguageChanged = m_currentLanguage != value && m_currentLanguage != SystemLanguage.Unknown;
			m_currentLanguage = value;
		}
	}

	private const string SavedLanguageKey = "Settings.CurrentLanguage";

	public string PathToLanguages
	{
		get { return m_pathToLanguages; }
	}

	public List<SystemLanguage> SupportedLanguages
	{
		get { return m_supportedLanguages; }
	}

	public List<SystemLanguage> LanguagesForLocalizationBuild
	{
		get { return m_languagesForLocalizationBuild; }
	}

	public bool CurrentLanguageChanged { get; set; }

	public SystemLanguage DefaultLanguage
	{
		get { return m_defaultLanguage; }
	}

	[SerializeField]
	private bool m_useOnlyDefaults = false;

	[SerializeField]
	private string m_pathToLanguages = @"Configs/Languages/";

	[SerializeField]
	private SystemLanguage m_defaultLanguage = SystemLanguage.English;

	[SerializeField]
	private List<SystemLanguage> m_supportedLanguages = new List<SystemLanguage>();

	[SerializeField]
	private TextManagerReplaceLanguages m_replaceLanguages = null;

	[SerializeField]
	private List<SystemLanguage> m_languagesForLocalizationBuild = new List<SystemLanguage>();

	private XmlSerializableDictionary m_dictionary = new XmlSerializableDictionary();
	private SystemLanguage m_currentLanguage = SystemLanguage.Unknown;
}