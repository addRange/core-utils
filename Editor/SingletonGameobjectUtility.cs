//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (3/19/2016)
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Core.Utils;
using UnityEditor;
using UnityEngine;

/// <summary>
/// TODO: check create prefab on first import (or after long time import)
/// </summary>
[InitializeOnLoad]
public static class SingletonGameobjectUtility
{
	static SingletonGameobjectUtility()
	{
		//Debug.Log("SingletonGameobjectUtility " + EditorApplication.isUpdating + "; " + EditorApplication.isCompiling); // always true, true
		List<Type> types = new List<Type>();
		var asms = AppDomain.CurrentDomain.GetAssemblies();
		foreach (var assembly in asms)
		{
			if (assembly.FullName.StartsWith("Unity"))
			{
				continue;
			}
			if (assembly.FullName.StartsWith("ExCSS.Unity"))
			{
				continue;
			}
			if (assembly.FullName.StartsWith("Mono"))
			{
				continue;
			}
			if (assembly.FullName.StartsWith("mscorlib"))
			{
				continue;
			}
			if (assembly.FullName.StartsWith("Vuforia"))
			{
				continue;
			}
			if (assembly.FullName.StartsWith("System"))
			{
				continue;
			}
			if (assembly.FullName.StartsWith("Facebook"))
			{
				continue;
			}
			if (assembly.FullName.StartsWith("ConsoleE"))
			{
				continue;
			}
			if (assembly.FullName.StartsWith("nunit.framework"))
			{
				continue;
			}

			var allTypes = assembly.GetTypes().ToList();
			types.AddRange(allTypes);
			//Debug.Log(assembly.FullName + ":");
			//allTypes.Sort((a, b) => a.FullName.CompareTo(b.FullName));
			//foreach (var allType in allTypes)
			//{
			//	Debug.Log("\t" + allType.FullName);
			//}
		}

		var sgoType = typeof(SingletonGameObject<>);
		var sgos = new List<Type>();
		sgos.Sort();
		foreach (var type in types)
		{
			//bool isDebug = type.FullName.Contains("Profile");
			//if (isDebug) { Debug.Log(type.FullName); }
			if (type.BaseType == null)
			{
				//if (isDebug) { Debug.Log(type + ". baseTypeNULL"); }
				continue;
			}
			if (type.IsAbstract)
			{
				//if (isDebug) { Debug.Log(type + ". IsAbstract"); }
				continue;
			}
			if (sgoType == type)
			{
				//if (isDebug) { Debug.Log(type + ". type==" + sgoType); }
				continue;
			}
			if (!type.BaseType.IsGenericType)
			{
				//if (isDebug) { Debug.Log(type + ". not IsGenericType"); }
				continue;
			}

			var baseType = type.BaseType;
			bool isValid = false;
			while (baseType != null)
			{
				if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == sgoType)
				{
					//Debug.Log("Valid " + type.FullName + "; on " + baseType.FullName);
					isValid = true;
					break;
				}
				baseType = baseType.BaseType;
			}
			if (!isValid)
			{
				//if (isDebug) { Debug.Log(type + ". not inherit from"); }
				continue;
			}
			sgos.Add(type);
			var method = type.GetMethod("GetPathToPrefab",
												 BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
			string pathToPrefab = string.Empty;
			try
			{
				pathToPrefab = method.Invoke(null, null).ToString();
			}
			catch (Exception exception)
			{
				Debug.LogError("Error on type=" + type + ": " + exception);
				return;
			}

			string prefsKey = Application.identifier + "." + pathToPrefab;
			//if (isDebug) Debug.Log(pathToPrefab);
			//Debug.Log(type.FullName + "\r\nprefsKey=" + prefsKey + "\r\n" + type.ToString() + "\r\n\r\n");
			//continue;
			if (EditorPrefs.GetBool(prefsKey, false))
			{
				continue;
			}

			var obj = Resources.Load(pathToPrefab);
			if (obj != null)
			{
				EditorPrefs.SetBool(prefsKey, true);
				continue;
			}
			var pathToPrefabWithExt = Application.dataPath + "/Resources/" + pathToPrefab + ".prefab";
			if (File.Exists(pathToPrefabWithExt))
			{
				EditorPrefs.SetBool(prefsKey, true);
				continue;
			}

			GameObject go = new GameObject(type.Name);
			go.AddComponent(type);
			string fullPath = "Assets/Resources/" + pathToPrefab + ".prefab";
			ScriptableObjectUtility.CreateFolderIfNotExit(fullPath);
			var prefab = PrefabUtility.CreatePrefab(fullPath, go);
			GameObject.DestroyImmediate(go);
			Debug.Log("Created: " + pathToPrefab, prefab);
		}
	}
}