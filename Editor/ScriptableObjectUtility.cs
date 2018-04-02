// http://wiki.unity3d.com/index.php?title=CreateScriptableObjectAsset
// modifed by Zanleo

using System;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Core.Utils
{
	public static class ScriptableObjectUtility
	{
		/// <summary>
		//	This makes it easy to create, name and place unique new ScriptableObject asset files.
		/// </summary>
		public static T CreateAsset<T>(string path = null, string name = null, bool withLog = false)
			where T : ScriptableObject
		{
			T asset = ScriptableObject.CreateInstance<T>();
			CreateAsset(asset, path, name, withLog);
			return asset;
		}

		public static ScriptableObject CreateAsset(Type type, string path = null, string name = null, bool withLog = false)
		{
			var asset = ScriptableObject.CreateInstance(type);
			CreateAsset(asset, path, name, withLog);
			return asset;
		}

		private static void CreateAsset(ScriptableObject asset, string path = null, string name = null, bool withLog = false)
		{
			//Debug.Log(path + "; " + name + "; " + asset);
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
				FixName(ref name, asset);
				FixFolderEnd(ref path);
				TryAddName(ref path, name);
				FixPathExtension(ref path);
				path = AssetDatabase.GenerateUniqueAssetPath(path);
			}
			else
			{
				FixFolderEnd(ref path);
				FixName(ref name, asset);
				TryAddName(ref path, name);
			}
			if (!path.StartsWith("Assets/"))
			{
				path = "Assets/" + path;
			}
			FixPathExtension(ref path);
			CreateFolderIfNotExit(path);
			AssetDatabase.CreateAsset(asset, path);
			Undo.RegisterCreatedObjectUndo(asset, "Create object");
			if (withLog)
			{
				Debug.Log("Created " + asset, asset);
			}
			AssetDatabase.Refresh();
		}

		private static void FixName(ref string name, ScriptableObject asset)
		{
			if (string.IsNullOrEmpty(name))
			{
				name = "New" + asset.GetType().ToString();
			}
		}

		private static void FixFolderEnd(ref string path)
		{
			if (!path.EndsWith("/"))
			{
				path += "/";
			}
		}

		private static void TryAddName(ref string path, string name)
		{
			if (!string.IsNullOrEmpty(name))
			{
				path += name;
			}
		}

		private static void FixPathExtension(ref string path)
		{
			if (!path.EndsWith(".asset"))
			{
				path += ".asset";
			}
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
}