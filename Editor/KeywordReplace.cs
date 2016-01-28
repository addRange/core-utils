// Script from http://forum.unity3d.com/threads/c-script-template-how-to-make-custom-changes.273191/
// modifed by Zanleo

using UnityEngine;
using UnityEditor;

public class KeywordReplace : UnityEditor.AssetModificationProcessor
{
	public static void OnWillCreateAsset(string path)
	{
		path = path.Replace(".meta", "");
		int index = path.LastIndexOf(".");
		if (index < 0)
		{
			return;
		}
		string file = path.Substring(index);
		if (file != ".cs")
		{
			return;
		}
		index = Application.dataPath.LastIndexOf("Assets");
		path = Application.dataPath.Substring(0, index) + path;
		file = System.IO.File.ReadAllText(path);
		index = path.LastIndexOf("/");
		string editorScriptsName = path.Substring(index + 1);
		editorScriptsName = editorScriptsName.Substring(0, Mathf.Max(editorScriptsName.Length - "Editor.cs".Length, 0));

		file = file.Replace("#SCRIPTEDITOR#", editorScriptsName);
		file = file.Replace("#CREATIONDATE#", System.DateTime.Now.ToShortDateString());
		file = file.Replace("#PROJECTNAME#", PlayerSettings.productName);
		file = file.Replace("#SMARTDEVELOPERS#", PlayerSettings.companyName);

		System.IO.File.WriteAllText(path, file);
		AssetDatabase.Refresh();
	}
}
