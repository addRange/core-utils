//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (2018.05.30)
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Used for inEditor funcs only
/// </summary>
[Serializable]
[FilePath("Assets/Editor/Configs/SingletonGameObjectsConfig.asset")]
public class SingletonGameObjectsConfig : SingletonScriptableObject<SingletonGameObjectsConfig>
{
	[Serializable]
	public class Config
	{
		public Config()
		{
		}

		public Config(Type type, bool? enabled = null)
		{
			Name = type.Name;
			FullName = type.FullName;
			if (enabled.HasValue)
			{
				Enabled = enabled.Value;
			}
			else
			{
				Enabled = EditorDefaultEnabled(type);
			}
		}
		
		private bool EditorDefaultEnabled(Type type)
		{
#if UNITY_EDITOR
			var guids = AssetDatabase.FindAssets("t:script " + type.Name);
			string targetScriptPath = null;
			foreach (var guid in guids)
			{
				var path = AssetDatabase.GUIDToAssetPath(guid);
				var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
				var monoScript = asset as MonoScript;
				if (monoScript == null)
				{
					continue;
				}
				if (monoScript.GetClass() != type)
				{
					continue;
				}
				targetScriptPath = path;
				break;
			}

			bool isCoreUtilsScripts = false;
			if (string.IsNullOrEmpty(targetScriptPath))
			{
				return true;
			}

			var coreUtilsPath = PathExt.Combine("Assets", "Plugins", "CoreUtils");
			coreUtilsPath = coreUtilsPath.Replace("\\", "/");
			var slashedPath = targetScriptPath.Replace("\\", "/");
			isCoreUtilsScripts = slashedPath.StartsWith(coreUtilsPath);
			//Debug.Log("\tFound path " + targetScriptPath + "; " + slashedPath.StartsWith(path));	

			if (!isCoreUtilsScripts)
			{
				return true;
			}

			return false;
#else
			return true;
#endif
		}

		public string Name
		{
			get { return m_name; }
			private set { m_name = value; }
		}

		public string FullName
		{
			get { return m_fullName; }
			private set { m_fullName = value; }
		}

		public bool Enabled
		{
			get { return m_enabled; }
			set { m_enabled = value; }
		}
		
		[SerializeField, NotEditable]
		private string m_name = string.Empty;
		[SerializeField, NotEditable]
		private string m_fullName = string.Empty;
		[SerializeField]
		private bool m_enabled = true;
	}

	/// <summary>
	/// Indexer with autoCreate config (auto set enabled by source)
	/// </summary>
	/// <param name="type"></param>
	/// <returns></returns>
	public Config this[Type type]
	{
		get
		{
			var res = Configs.Find(c => c.FullName == type.FullName);
			if (res == null)
			{
				res = new Config(type);
				Configs.Add(res);
			}
			return res;
		}
	}

	/// <summary>
	/// Indexer with autoCreate config (auto set enabled by source)
	/// </summary>
	/// <param name="type"></param>
	/// <returns></returns>
	public Config this[Object obj]
	{
		get
		{
			return this[obj.GetType()];
		}
	}

	private List<Config> Configs
	{
		get { return m_configs; }
	}

	[Header("After change some settings - need recompile scripts")]
	[SerializeField]
	private List<Config> m_configs = new List<Config>();
}