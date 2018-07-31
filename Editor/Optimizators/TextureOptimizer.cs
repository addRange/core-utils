//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (02/03/2016)
//-----------------------------------------------------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

class TextureOptimizer : EditorWindow
{
	[MenuItem("Window/Utils/Textures Optimizer")]
	public static void ShowWindow()
	{
		EditorWindow.GetWindow(typeof(TextureOptimizer), false, "Textures Optimizer", true);
	}

	private void OnEnable()
	{
		UpdateObjectsForOptimize();
		EditorApplication.hierarchyChanged += OnProjectWindowChanged;
	}

	private void OnDisable()
	{
		EditorApplication.hierarchyChanged -= OnProjectWindowChanged;
	}

	private void OnProjectWindowChanged()
	{
		if (m_inChangingProcess)
		{
			return;
		}

		UpdateObjectsForOptimize();
	}

	private void UpdateObjectsForOptimize()
	{
		//AssetDatabase.FindAssets()
		//AssetDatabase.LoadAssetAtPath<>()
		m_allTextures.Clear();
		m_allTexturePathImporter.Clear();
		var allTexturesGuids =
			AssetDatabase.FindAssets("t:Texture")
							 .ToList(); //Resources.LoadAll<Texture>("").ToList();//Resources.FindObjectsOfTypeAll<Texture>().ToList();
		var paths = allTexturesGuids.Select<string, string>(s => AssetDatabase.GUIDToAssetPath(s)).ToList();
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
			EditorUtility.DisplayProgressBar("Disable MipMaps", "disable mipmaps", 0);
			int curIndex = 0;
			m_inChangingProcess = true;
			foreach (var texturePair in m_allTexturePathImporter)
			{
				float progress = (float) curIndex / (float) m_allTexturePathImporter.Count;
				if (EditorUtility.DisplayCancelableProgressBar("Disable MipMaps", texturePair.Key, progress))
				{
					Debug.LogError("Canceled by user");
					break;
				}

				if (!texturePair.Value.mipmapEnabled)
				{
					continue;
				}

				DisableMipMaps(texturePair.Value);
				curIndex++;
			}

			EditorUtility.ClearProgressBar();
			AssetDatabase.Refresh();
			m_inChangingProcess = false;
			return;
		}

		m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos);
		foreach (var texturePair in m_allTexturePathImporter)
		{
			if (texturePair.Value == null)
			{
				continue;
			}
			if (!texturePair.Value.mipmapEnabled)
			{
				continue;
			}

			EditorGUILayout.BeginHorizontal();
			if (!GUILayout.Toggle(true, "", m_maxToggleWidth))
			{
				m_inChangingProcess = true;
				DisableMipMaps(texturePair.Value);
				m_inChangingProcess = false;
			}
			if (GUILayout.Button(texturePair.Key))
			{
				var res = AssetDatabase.LoadAssetAtPath<Object>(texturePair.Value.assetPath);
				Selection.activeObject = res;
			}
			EditorGUILayout.EndHorizontal();
		}

		EditorGUILayout.EndScrollView();
	}

	private void DisableMipMaps(TextureImporter importer)
	{
		importer.ReadTextureSettings(m_settings); //and make success false so the process can abort
		m_settings.mipmapEnabled = false;
		importer.SetTextureSettings(m_settings);
		importer.SaveAndReimport();
	}

	private Vector2 m_scrollPos = Vector2.zero;
	private List<Texture> m_allTextures = new List<Texture>();
	private Dictionary<string, TextureImporter> m_allTexturePathImporter = new Dictionary<string, TextureImporter>();
	private GUILayoutOption m_maxToggleWidth = GUILayout.MaxWidth(20);
	private TextureImporterSettings m_settings = new TextureImporterSettings();
	private bool m_inChangingProcess = false;
}