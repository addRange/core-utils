//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (1/13/2016)
//----------------------------------------------------------------------------------------------

using System.Linq;
using Core.Utils;
using Effects.Player;
using UnityEditor;
using UnityEngine;
using Assert = Core.Utils.Assert;

namespace Effects
{
	[CustomEditor(typeof(EffectManager))]
	public class EffectManagerEditor : Editor
	{
		private void OnEnable()
		{
			Target = (EffectManager)target;
			EffectsPropertiesProperty = serializedObject.FindProperty("m_effectsProperties");
			if (EffectsProperties == null)
			{
				EffectsPropertiesProperty.objectReferenceValue = CreateEffectsProperties("Game", false);
				serializedObject.ApplyModifiedPropertiesWithoutUndo();
			}
		}

		private void OnDisable()
		{
			LastEffectsProperties = null;
			EffectsPropertiesProperty = null;
			Target = null;
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			LastEffectsProperties = EffectsProperties;
			//EditorGUILayout.PropertyField(EffectsPropertiesProperty, true);
			EditorGUI.indentLevel++;
			m_effectsPropertiesEditor.OnInspectorGUI();
			EditorGUI.indentLevel--;
			if (GUILayout.Button("AutoFill"))
			{
				AutoFill();
			}
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("SortByName"))
			{
				EffectsProperties.Sort(SorterByName);
			}
			if (GUILayout.Button("SortByPath"))
			{
				EffectsProperties.Sort(SorterByPath);
			}
			GUILayout.EndHorizontal();
			if (GUI.changed)
			{
				EditorUtility.SetDirty(Target);
				EditorUtility.SetDirty(LastEffectsProperties);
			}
		}

		private void AutoFill()
		{
			var path = serializedObject.FindProperty("m_path").stringValue;
			Debug.Log("Start load effects from path: " + path);
			var effectsResources = Resources.LoadAll<GameObject>(path);
			var listEffects = effectsResources.ToList();
			// Remove already added
			for (int i = 0; i < EffectsProperties.Count; ++i)
			{
				var curEffectProp = EffectsProperties[i];
				if (Resources.Load<GameObject>(path + curEffectProp.EffectPath) == null)
				{
					Debug.LogError("Delete, because not found " + curEffectProp.EffectPath);
					EffectsProperties.Remove(curEffectProp);
					--i;
				}
			}
			for (int i = 0; i < EffectsProperties.Count; i++)
			{
				var founded = listEffects.Find(er => er.name.ToLower() == EffectsProperties[i].EffectName.ToLower());
				if (founded == null)
				{
					continue;
				}
				var effectPlayer = founded.GetComponent<EffectPlayer>();
				if (effectPlayer == null)
				{
					Debug.LogAssertion("Wrong prefab withou EffectPlayer", founded);
					listEffects.Remove(founded);
					continue;
				}
				var foundedPath = GetEffectPath(path, founded);
				if (EffectsProperties[i].EffectPath.ToLower() != foundedPath.ToLower())
				{
					Debug.Log("Fix Path of " + EffectsProperties[i].EffectName + " from \"" +
								 EffectsProperties[i].EffectPath + "\" to \"" + foundedPath + "\"", founded);
					EffectsProperties[i].SetEditorEffectPath(foundedPath);
				}
				listEffects.Remove(founded);
				Assert.IsNull(listEffects.Find(er => er.name.ToLower() == EffectsProperties[i].EffectName.ToLower()),
					"Found effect duplicate name " + EffectsProperties[i].EffectName);
			}
			for (int i = 0; i < listEffects.Count; i++)
			{
				var effectRes = listEffects[i];
				Debug.Log("AutoAdd " + effectRes, effectRes);
				var newProp = new EffectProperty(effectRes.name, GetEffectPath(path, effectRes), GamePlace.Game);
				EffectsProperties.Add(newProp);
			}
			Debug.Log("End AutoFill");
		}

		private int SorterByName(EffectProperty x, EffectProperty y) { return x.EffectName.CompareTo(y.EffectName); }
		private int SorterByPath(EffectProperty x, EffectProperty y) { return x.EffectPath.CompareTo(y.EffectPath); }

		private string GetEffectPath(string effectsPath, GameObject effectRes)
		{
			var resPath = AssetDatabase.GetAssetPath(effectRes);
			resPath = resPath.Replace("Assets/Resources/" + effectsPath, "");
			resPath = resPath.Replace(".prefab", "");
			return resPath;
		}

		private EffectsProperties CreateEffectsProperties(string path, bool isMiniGame)
		{
			SerializedProperty pathProp = null;
			if (isMiniGame)
			{
				pathProp = serializedObject.FindProperty("m_minigamePath");
			}
			else
			{
				pathProp = serializedObject.FindProperty("m_path");
			}

			return ScriptableObjectUtility.CreateAsset<EffectsProperties>("Assets/Resources/" + pathProp.stringValue + path, withLog: true);
		}

		private EffectManager Target { get; set; }
		private SerializedProperty EffectsPropertiesProperty { get; set; }

		private EffectsProperties EffectsProperties
		{
			get
			{
				return EffectsPropertiesProperty.objectReferenceValue as EffectsProperties;
			}
		}

		private EffectsProperties LastEffectsProperties
		{
			get
			{
				return EffectsPropertiesProperty.objectReferenceValue as EffectsProperties;
			}
			set
			{
				if (m_lastEffectsProperties == value)
				{
					return;
				}
				m_lastEffectsProperties = value;
				if (m_effectsPropertiesEditor != null)
				{
					DestroyImmediate(m_effectsPropertiesEditor);
				}
				m_effectsPropertiesEditor = CreateEditor(m_lastEffectsProperties);
			}
		}

		private EffectsProperties m_lastEffectsProperties = null;
		private Editor m_effectsPropertiesEditor = null;
	}
}