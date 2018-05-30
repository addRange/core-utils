//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (2018.05.30)
//----------------------------------------------------------------------------------------------

//#define Debug_SingletonScriptableObject

using System.Diagnostics;
using System.IO;
using Core.Utils;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public abstract class SingletonScriptableObject<T> : ScriptableObject where T : SingletonScriptableObject<T>
{
	[Conditional("Debug_SingletonScriptableObject"), DebuggerStepThrough]
	private static void Log(string msg, Object context = null)
	{
		Debug.Log(msg, context);
	}

	protected SingletonScriptableObject()
	{
	}

	private static void LoadOrCreate()
	{
		string filePath = GetPathToAsset();
		Assert.IsNotNull(filePath, "null path returned for " + typeof(T));
#if UNITY_EDITOR
		s_instance = AssetDatabase.LoadAssetAtPath<T>(filePath);
#endif

		if (s_instance == null)
		{
			s_instance = Resources.Load<T>(filePath);
		}

#if UNITY_EDITOR
		if (s_instance == null)
		{
			s_instance = CreateInstance<T>();
			var relativePath = filePath;
			if (relativePath.StartsWith("Assets"))
			{
				relativePath = relativePath.Substring("Assets/".Length);
			}
			var fullPath = Application.dataPath + "/" + relativePath;
			var dirPath = Path.GetDirectoryName(fullPath);
			Directory.CreateDirectory(dirPath);
			AssetDatabase.CreateAsset(s_instance, filePath);
			Debug.Log("AutoCreated " + typeof(T) + " by path " + filePath, s_instance);
		}
#endif
	}

	private static string GetPathToAsset()
	{
#if UNITY_EDITOR
		foreach (object customAttribute in typeof(T).GetCustomAttributes(true))
		{
			if (customAttribute is FilePathAttribute)
			{
				return (customAttribute as FilePathAttribute).FilePath;
			}
		}
#endif
		string pathToRes = typeof(T).ToString().Replace('.', '/');
		pathToRes = PathToConfigs + pathToRes;
		return pathToRes;
	}

	public static T Instance
	{
		get
		{
			if (s_instance == null)
			{
				LoadOrCreate();
			}

			return s_instance;
		}
	}

	protected static string PathToConfigs = "Configs/Scripts/";

	private static T s_instance = null;
}