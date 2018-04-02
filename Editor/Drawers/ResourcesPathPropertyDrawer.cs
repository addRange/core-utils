//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (1/14/2016)
//----------------------------------------------------------------------------------------------

using System.IO;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ResourcesPathAttribute), true)]
public class ResourcesPathPropertyDrawer : PropertyDrawer
{
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return base.GetPropertyHeight(property, label) * 2.0f;
	}

	public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, prop);

		var attr = (ResourcesPathAttribute) attribute;
		string path = "Assets/Resources/" + prop.stringValue;
		UnityEngine.Object obj = null;

		if (Directory.Exists(Path.GetDirectoryName(path)))
		{
			string[] pathes = Directory.GetFiles(Path.GetDirectoryName(path), Path.GetFileName(path) + ".*");
			if (pathes.Length != 0)
			{
				obj = AssetDatabase.LoadAssetAtPath(pathes[0], attr.AssetType);
			}
		}
		position.yMax -= position.height / 2.0f;
		string labelText = label.text;
		UnityEngine.Object newObj = EditorGUI.ObjectField(position, labelText + " (" + attr.AssetType.Name + ")", obj,
																		  attr.AssetType, false);
		if (obj != newObj)
		{
			string str = AssetDatabase.GetAssetPath(newObj);
			if (str.StartsWith("Assets/Resources/", true, null))
			{
				str = str.Remove(0, "Assets/Resources/".Length);
				str = str.Remove(str.LastIndexOf("."), str.Length - str.LastIndexOf("."));
				prop.stringValue = str;
			}
			else
			{
				Debug.LogWarning("Asset must be located in 'resources' folder only!");
			}
		}

		position.yMax += position.height;
		position.yMin += position.height / 2.0f;
		prop.stringValue = EditorGUI.TextField(position, labelText + " (Path)", prop.stringValue);

		EditorGUI.EndProperty();
	}
}