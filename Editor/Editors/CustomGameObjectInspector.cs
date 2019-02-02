//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (2017.01.27)
//----------------------------------------------------------------------------------------------

#pragma warning disable 0618

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

#if !UNITY_2018_3_OR_NEWER
[CanEditMultipleObjects, CustomEditor(typeof(GameObject))]
#endif
public class CustomGameObjectInspector : Editor
{
	private static Action EventExitGUI = null;

	public override bool HasPreviewGUI()
	{
		if (m_unityEditor != null)
		{
			return m_unityEditor.HasPreviewGUI();
		}

		return base.HasPreviewGUI();
	}

	public override void DrawPreview(Rect previewArea)
	{
		if (m_unityEditor != null)
		{
			m_unityEditor.DrawPreview(previewArea);
			return;
		}

		base.DrawPreview(previewArea);
	}

	public override string GetInfoString()
	{
		if (m_unityEditor != null)
		{
			return m_unityEditor.GetInfoString();
		}

		return base.GetInfoString();
	}

	public override GUIContent GetPreviewTitle()
	{
		if (m_unityEditor != null)
		{
			return m_unityEditor.GetPreviewTitle();
		}

		return base.GetPreviewTitle();
	}

	public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
	{
		if (m_unityEditor != null)
		{
			m_unityEditor.OnInteractivePreviewGUI(r, background);
			return;
		}

		base.OnInteractivePreviewGUI(r, background);
	}

	public override bool UseDefaultMargins()
	{
		if (m_unityEditor != null)
		{
			return m_unityEditor.UseDefaultMargins();
		}

		return base.UseDefaultMargins();
	}

	public override bool RequiresConstantRepaint()
	{
		if (m_unityEditor != null)
		{
			return m_unityEditor.RequiresConstantRepaint();
		}

		return base.RequiresConstantRepaint();
	}

	public override void OnPreviewSettings()
	{
		if (m_unityEditor != null)
		{
			m_unityEditor.OnPreviewSettings();
			return;
		}

		base.OnPreviewSettings();
	}

	public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
	{
		if (m_unityEditor != null)
		{
			return m_unityEditor.RenderStaticPreview(assetPath, subAssets, width, height);
		}

		return base.RenderStaticPreview(assetPath, subAssets, width, height);
	}

	public override void OnPreviewGUI(Rect r, GUIStyle background)
	{
		if (m_unityEditor != null)
		{
			m_unityEditor.OnPreviewGUI(r, background);
			return;
		}

		base.OnPreviewGUI(r, background);
	}

	public override void ReloadPreviewInstances()
	{
		if (m_unityEditor != null)
		{
			m_unityEditor.ReloadPreviewInstances();
			return;
		}

		base.ReloadPreviewInstances();
	}

	public void OnDestroy()
	{
		DestroyEditor();
	}

	public void OnEnable()
	{
		EventExitGUI += OnExitGUI;
	}

	public void OnDisable()
	{
		EventExitGUI -= OnExitGUI;
		DestroyEditor();
		m_unityEditorOnHeaderGUI = null;
	}

	private void OnExitGUI()
	{
		DestroyEditor();
	}

	private void CreateNewEditor()
	{
		var assembly = Assembly.GetAssembly(typeof(Editor));
		var unityEditorType = assembly.GetType("UnityEditor.GameObjectInspector", true);
		if (targets.Length > 1)
		{
			m_unityEditor = Editor.CreateEditor(targets, unityEditorType);
		}
		else
		{
			m_unityEditor = Editor.CreateEditor(target, unityEditorType);
		}
		m_unityEditorOnHeaderGUI = unityEditorType.GetMethod("OnHeaderGUI",
																			  System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public |
																			  System.Reflection.BindingFlags.NonPublic);
		//Debug.Log("CreateNewEditor " + GetInfo());
	}

	private void DestroyEditor()
	{
		//Debug.Log("DestroyEditor " + GetInfo());
		if (m_unityEditor != null)
		{
			DestroyImmediate(m_unityEditor);
			m_unityEditor = null;
		}
	}

	private object GetInfo()
	{
		var res = GetHashCode() + "; " + (target != null) + "; " + targets.Length + "; " + (m_unityEditor != null);
		try
		{
			res += "; " + m_unityEditor.GetHashCode();
		}
		catch
		{
			// ignored
		}
		return res;
	}

	protected override void OnHeaderGUI()
	{
		if (m_unityEditor == null)
		{
			CreateNewEditor();
		}
		if (m_unityEditorOnHeaderGUI != null)
		{
			m_unityEditorOnHeaderGUI.Invoke(m_unityEditor, null);
		}
		else
		{
			base.OnHeaderGUI();
		}
		if (targets.Length > 1)
		{
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Prefab", GUILayout.ExpandWidth(false));
			if (GUILayout.Button("Apply", (GUIStyle) "MiniButtonRight", new GUILayoutOption[0]))
			{
				Debug.Log("Start apply on targets " + targets.Length);
				var prefabsForApply = new Dictionary<GameObject, KeyValuePair<Object, Object>>();
				foreach (var targObj in targets)
				{
					UnityEngine.Object prefabParent = PrefabUtility.GetCorrespondingObjectFromSource(targObj);
					if (prefabParent == null)
					{
						Debug.Log("prefabParent null. Skip " + targObj, targObj);
						continue;
					}

					GameObject prefabInstanceRoot = PrefabUtility.FindValidUploadPrefabInstanceRoot(targObj as GameObject);
					if (prefabInstanceRoot == null)
					{
						Debug.Log("prefabInstanceRoot null. Skip " + targObj, targObj);
						continue;
					}

					if (prefabsForApply.ContainsKey(prefabInstanceRoot))
					{
						var pathAdded = GetScenePath(prefabsForApply[prefabInstanceRoot].Value as GameObject);
						var pathCur = GetScenePath(targObj as GameObject);
						if (pathAdded.Length > pathCur.Length)
						{
							prefabsForApply.Remove(prefabInstanceRoot);
						}
						else
						{
							continue;
						}
					}

					prefabsForApply.Add(prefabInstanceRoot, new KeyValuePair<Object, Object>(prefabParent, targObj));
				}
				foreach (var pairPrefabRoot in prefabsForApply)
				{
					var prefabInstanceRoot = pairPrefabRoot.Key;
					var prefabParent = pairPrefabRoot.Value.Key;
					var targObj = pairPrefabRoot.Value.Value;
					PrefabUtility.ReplacePrefab(prefabInstanceRoot, prefabParent, ReplacePrefabOptions.ConnectToPrefab);
					EditorSceneManager.MarkSceneDirty(prefabInstanceRoot.scene);
					Debug.Log("Applied on " + GetScenePath(targObj as GameObject), prefabInstanceRoot);
				}

				EventExitGUI.Invoke();
				GUIUtility.ExitGUI();
			}

			EditorGUILayout.EndHorizontal();
		}
	}

	public static string GetScenePath(GameObject forObj)
	{
		string path = forObj.name;
		var transform = forObj.transform.parent;
		while (transform != null)
		{
			path = transform.name + "/" + path;
			transform = transform.parent;
		}

		return path;
	}

	public override void OnInspectorGUI()
	{
		if (m_unityEditor == null)
		{
			CreateNewEditor();
		}
		if (m_unityEditor != null)
		{
			m_unityEditor.OnInspectorGUI();
		}
		else
		{
			base.OnInspectorGUI();
		}
	}

	private Editor m_unityEditor = null;
	private MethodInfo m_unityEditorOnHeaderGUI = null;
}

#pragma warning restore 0618