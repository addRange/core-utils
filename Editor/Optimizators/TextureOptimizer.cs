//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (02/03/2016)
//-----------------------------------------------------------------------------------------

using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

class TextureOptimizer : EditorWindow
{
	[MenuItem("Window/Textures Optimizer")]
	public static void ShowWindow()
	{
		EditorWindow.GetWindow(typeof(TextureOptimizer), false, "Textures Optimizer", true);
	}

	private void OnEnable()
	{
		UpdateObjectsForOptimize();
		EditorApplication.hierarchyWindowChanged += OnHierarchyWindowChanged;
	}

	private void OnDisable()
	{
		EditorApplication.hierarchyWindowChanged -= OnHierarchyWindowChanged;
	}

	private void OnHierarchyWindowChanged()
	{
		//if (m_inChangingProcess)
		{
			//return;
		}
		UpdateObjectsForOptimize();
	}

	private void UpdateObjectsForOptimize()
	{
		//AssetDatabase.FindAssets()
		//AssetDatabase.LoadAssetAtPath<>()
		m_allTextures.Clear();

		var allTexturesGuids = AssetDatabase.FindAssets("t:Texture").ToList();//Resources.LoadAll<Texture>("").ToList();//Resources.FindObjectsOfTypeAll<Texture>().ToList();
		var paths = allTexturesGuids.Select<string,string>(s => AssetDatabase.GUIDToAssetPath(s)).ToList();
		foreach (var path in paths)
		{
			//var allTexture = AssetDatabase.LoadAssetAtPath<Texture>(path);
			//m_allTextures.Add(allTexture);
			m_allTexturePathImporter.Add(path, AssetImporter.GetAtPath(path) as TextureImporter);
		}
	}
	
	private void OnGUI()
	{
		if (GUILayout.Button("Not generate MipMaps for all"))
		{
			foreach (var texturePair in m_allTexturePathImporter)
			{
				DisableMipMaps(texturePair.Value);
			}
			AssetDatabase.Refresh();
			return;
		}
		m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos);
		foreach (var texturePair in m_allTexturePathImporter)
		{
			if (!texturePair.Value.mipmapEnabled)
			{
				continue;
			}
			EditorGUILayout.BeginHorizontal();
			if (!GUILayout.Toggle(true, "", m_maxToggleWidth))
			{
				DisableMipMaps(texturePair.Value);
			}
			if (GUILayout.Button(texturePair.Key))
			{
				// TODO?
				//Selection.activeObject = textureImporter.Key;
			}
			EditorGUILayout.EndHorizontal();
		}
// 		foreach (var allTexture in m_allTextures)
// 		{
// 			if (GUILayout.Button(allTexture.name))
// 			{
// 				Selection.activeObject = allTexture;
// 			}
// 		}
		EditorGUILayout.EndScrollView();
	}

	private void DisableMipMaps(TextureImporter importer)
	{
		importer.ReadTextureSettings(m_settings);	//and make success false so the process can abort
		m_settings.mipmapEnabled = false;
		importer.SetTextureSettings(m_settings);
		importer.SaveAndReimport();
	}

	private Vector2 m_scrollPos = Vector2.zero;
	private List<Texture> m_allTextures = new List<Texture>();
	private Dictionary<string, TextureImporter> m_allTexturePathImporter = new Dictionary<string, TextureImporter>();
	private GUILayoutOption m_maxToggleWidth = GUILayout.MaxWidth(20);
	TextureImporterSettings m_settings = new TextureImporterSettings();
}
