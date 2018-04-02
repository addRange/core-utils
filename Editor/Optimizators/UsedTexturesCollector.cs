//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (02/03/2016)
//-----------------------------------------------------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

class UsedTexturesCollector : EditorWindow
{
	[MenuItem("Window/Utils/UsedTexturesCollector")]
	public static void ShowWindow()
	{
		EditorWindow.GetWindow(typeof(UsedTexturesCollector), false, "UsedTexturesCollector", true);
	}

	private void OnEnable()
	{
		m_allSprites.Clear();
		m_needAddCollected = true;
		EditorApplication.hierarchyWindowChanged += OnHierarchyWindowChanged;
		EditorApplication.update += OnUpdate;
	}

	private void OnDisable()
	{
		EditorApplication.update -= OnUpdate;
		EditorApplication.hierarchyWindowChanged -= OnHierarchyWindowChanged;
	}

	private void OnHierarchyWindowChanged()
	{
		m_needAddCollected = true;
	}

	private void OnUpdate()
	{
		if (!m_needAddCollected)
		{
			return;
		}

		m_needAddCollected = false;
		var imgs = GameObject.FindObjectsOfType<Image>();
		foreach (var image in imgs)
		{
			if (image.sprite == null)
			{
				continue;
			}
			if (!m_texturesPlaces.ContainsKey(image.sprite.texture))
			{
				m_texturesPlaces.Add(image.sprite.texture, new List<string>());
			}
			var list = m_texturesPlaces[image.sprite.texture];
			var sceneName = SceneManager.GetActiveScene().name;
			if (!list.Contains(sceneName))
			{
				list.Add(sceneName);
			}
			var canvas = image.gameObject.GetComponentInParent<Canvas>();
			if (canvas != null)
			{
				string panelName = canvas.gameObject.name;
				if (!list.Contains(panelName))
				{
					list.Add(panelName);
				}
			}
		}
	}

	private void OnGUI()
	{
		m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos);
		foreach (var allSprite in m_texturesPlaces)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.ObjectField(allSprite.Key.name, allSprite.Key, typeof(Texture), true);
			var list = string.Join(", ", allSprite.Value.ToArray());
			EditorGUILayout.LabelField(list);
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndScrollView();

		if (GUILayout.Button("Find All tags"))
		{
			var all = AssetDatabase.FindAssets("t:Texture2D");
			foreach (var textureGuid in all)
			{
				string path = AssetDatabase.GUIDToAssetPath(textureGuid);
				var textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
				if (textureImporter == null)
				{
					continue;
				}
				if (string.IsNullOrEmpty(textureImporter.spritePackingTag))
				{
					continue;
				}
				Debug.Log(textureImporter.spritePackingTag + "; " + path, textureImporter);
			}
		}
	}

	private List<Texture> m_allSprites = new List<Texture>();
	private Dictionary<Texture, List<string>> m_texturesPlaces = new Dictionary<Texture, List<string>>();
	private bool m_needAddCollected = false;
	private Vector2 m_scrollPos = Vector2.zero;
}