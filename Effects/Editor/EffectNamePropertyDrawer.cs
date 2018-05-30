//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (1/14/2016)
//----------------------------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Effects
{
	[CustomPropertyDrawer(typeof(EffectNameAttribute), true)]
	public class EffectNamePropertyDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			float h = base.GetPropertyHeight(property, label);
			var effMana = EffectManager.GetComponentOnly();
			if (effMana == null)
			{
				return h;
			}

			return h * 2f + 2f;
		}

		public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, prop);

			var effMana = EffectManager.GetComponentOnly();
			if (effMana == null)
			{
				EditorGUI.PropertyField(position, prop);
				EditorGUI.EndProperty();
				return;
			}

			CheckFillingEffects(effMana, prop);

			position.yMax -= position.height / 2.0f;
			string labelText = label.text;
			string lastVal = prop.stringValue;
			EditorGUI.PropertyField(position, prop);
			if (lastVal != prop.stringValue || m_lastCalculatedVal != prop.stringValue)
			{
				m_lastCalculatedVal = prop.stringValue;
				Filter(prop);
			}

			position.yMax += position.height + 2f;
			position.yMin += position.height / 2.0f;
			if (m_allEffects.Count == 0)
			{
				EditorGUI.LabelField(position, "Not found effect in EffectManager");
			}
			else
			{
				int newVal = EditorGUI.Popup(position, labelText + " (popup)", m_curSelectedIndex, m_sortedEffects.ToArray());
				if (newVal != m_curSelectedIndex)
				{
					m_curSelectedIndex = newVal;
					prop.stringValue = m_sortedEffects[m_curSelectedIndex];
					Filter(prop);
					m_curSelectedIndex = m_sortedEffects.IndexOf(prop.stringValue);
				}
			}

			EditorGUI.EndProperty();
		}

		private void CheckFillingEffects(EffectManager effMana, SerializedProperty prop)
		{
			if (effMana.Properties == null)
			{
				return;
			}

			if (effMana.Properties.Count != m_allEffects.Count)
			{
				m_allEffects.Clear();
				for (int i = 0; i < effMana.Properties.Count; ++i)
				{
					m_allEffects.Add(effMana.Properties[i].EffectName);
				}

				Filter(prop);
			}
		}

		private void Filter(SerializedProperty prop)
		{
			string newVal = prop.stringValue.ToLower();
			m_sortedEffects = m_allEffects.FindAll(aen => aen.ToLower().StartsWith(newVal));
			m_sortedEffects.Sort();
			m_curSelectedIndex = m_sortedEffects.IndexOf(prop.stringValue);
		}

		private List<string> m_allEffects = new List<string>();
		private List<string> m_sortedEffects = new List<string>();
		private int m_curSelectedIndex = 0;
		private string m_lastCalculatedVal = null;
	}
}