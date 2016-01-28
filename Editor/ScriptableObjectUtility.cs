// http://wiki.unity3d.com/index.php?title=CreateScriptableObjectAsset
// modifed by Zanleo

using System;
using UnityEngine;
using UnityEditor;
using System.IO;

public static class ScriptableObjectUtility
{
	/// <summary>
	//	This makes it easy to create, name and place unique new ScriptableObject asset files.
	/// </summary>
	public static T CreateAsset<T>(string path = null, bool withLog = false) where T : ScriptableObject
	{
		T asset = ScriptableObject.CreateInstance<T>();
		CreateAsset(asset, path, withLog);
		return asset;
	}

	public static ScriptableObject CreateAsset(Type type, string path = null, bool withLog = false)
	{
		var asset = ScriptableObject.CreateInstance(type);
		CreateAsset(asset, path, withLog);
		return asset;
	}

	private static void CreateAsset(ScriptableObject asset, string path = null, bool withLog = false)
	{
		if (string.IsNullOrEmpty(path))
		{
			path = AssetDatabase.GetAssetPath(Selection.activeObject);
			if (string.IsNullOrEmpty(path))
			{
				path = "Assets";
			}
			else if (Path.GetExtension(path) != "")
			{
				path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
			}
			path = AssetDatabase.GenerateUniqueAssetPath(path + "/New " + asset.GetType().ToString());
		}
		if (!path.EndsWith(".asset"))
		{
			path += ".asset";
		}
		CreateFolderIfNotExit(path);
		AssetDatabase.CreateAsset(asset, path);
		Undo.RegisterCreatedObjectUndo(asset, "Create object");
		if (withLog)
		{
			Debug.Log("Created " + asset, asset);
		}
		AssetDatabase.Refresh();
	}

	public static void CreateFolderIfNotExit(string path)
	{
		if (path.LastIndexOf("/") == -1)
		{
			return;
		}
		string fullPath = Application.dataPath;
		fullPath = fullPath.Substring(0, fullPath.LastIndexOf("/") + 1);
		fullPath += path;
		fullPath = fullPath.Substring(0, fullPath.LastIndexOf("/"));
		if (!Directory.Exists(fullPath))
		{
			Debug.Log("Created folder " + fullPath);
			Directory.CreateDirectory(fullPath);
		}
		AssetDatabase.Refresh();
	}
}