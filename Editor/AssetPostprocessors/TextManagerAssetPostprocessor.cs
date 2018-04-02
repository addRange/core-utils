//----------------------------------------------------------------------------------------------
// author Leonid [Zanleo] Voitko
//----------------------------------------------------------------------------------------------

using UnityEditor;
using UnityEngine;

/// <summary>
/// TODO: check create prefab on first import (or after long time import)
/// </summary>
public class TextManagerAssetPostprocessor : AssetPostprocessor
{
	private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
															 string[] movedFromAssetPaths)
	{
		//Debug.Log("OnPostprocessAllAssets " + EditorApplication.isUpdating + "; " + EditorApplication.isCompiling); // always true, true
		var textManager = TextManager.GetComponentOnly();
		if (textManager == null)
		{
			return;
		}

		bool needCheck = false;
		for (int i = 0; i < importedAssets.Length; i++)
		{
			needCheck |= BaseCheckPath(textManager, importedAssets[i]);
		}
		for (int i = 0; i < deletedAssets.Length; i++)
		{
			needCheck |= BaseCheckPath(textManager, deletedAssets[i]);
		}
		for (int i = 0; i < movedFromAssetPaths.Length; i++)
		{
			needCheck |= BaseCheckPath(textManager, movedFromAssetPaths[i]);
		}
		for (int i = 0; i < movedAssets.Length; i++)
		{
			needCheck |= BaseCheckPath(textManager, movedAssets[i]);
		}

		if (!needCheck)
		{
			textManager = null;
			return;
		}

		textManager.EditorCheckSupportedLanguages();
		textManager = null;
		Resources.UnloadUnusedAssets();
	}

	private static bool BaseCheckPath(TextManager textManager, string filePath)
	{
		if (!filePath.EndsWith(".txt"))
		{
			return false;
		}
		if (!filePath.StartsWith("Assets/Resources/" + textManager.PathToLanguages))
		{
			return false;
		}

		return true;
	}
}