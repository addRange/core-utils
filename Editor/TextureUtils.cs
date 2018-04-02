//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (12/27/2015)
//----------------------------------------------------------------------------------------------

using UnityEngine;
using UnityEditor;

public static class TextureUtils
{
	[MenuItem("Assets/Texture/InfoAbout", false)]
	private static void InfoAboutTexture()
	{
		foreach (var selectedObj in Selection.objects)
		{
			var selected = selectedObj as Texture2D;
			if (selected == null)
			{
				continue;
			}
			string path = AssetDatabase.GetAssetPath(selected);
			TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
			Debug.Log(textureImporter.GetAutomaticFormat("Android"));
			//textureImporter.
			Debug.Log("InfoAboutTexture " + selected, selected);
			Debug.Log("Original format " + selected.format);
		}
	}

	[MenuItem("Assets/Texture/", true)]
	[MenuItem("Assets/Texture/InfoAbout", true)]
	[MenuItem("Assets/Texture/Cut all scales", true)]
	[MenuItem("Assets/Texture/Create mirrored", true)]
	private static bool IsTexture()
	{
		if (Selection.activeObject == null)
		{
			return false;
		}

		return Selection.activeObject is Texture2D;
	}
}