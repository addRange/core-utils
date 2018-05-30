//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (1/13/2016)
//----------------------------------------------------------------------------------------------

using UnityEngine;
using System;
using System.Collections.Generic;

namespace Effects
{
	[Serializable]
	public class EffectsProperties : ScriptableObject
	{
		public void Add(EffectProperty effectProperty) { m_effectProperties.Add(effectProperty); }
		public void Remove(EffectProperty effectProperty) { m_effectProperties.Remove(effectProperty); }
		public EffectProperty Find(Predicate<EffectProperty> match) { return m_effectProperties.Find(match); }
		public void Sort(Comparison<EffectProperty> sorter) { m_effectProperties.Sort(sorter); }

		public EffectProperty this[int index] { get { return m_effectProperties[index]; } }
		public int Count { get { return m_effectProperties.Count; } }

		[SerializeField]
		private List<EffectProperty> m_effectProperties = new List<EffectProperty>();

		[ContextMenu("SetAllNotStaticMax")]
		private void EditorSetAllNotStaticMax()
		{
			for (int i = 0; i < this.Count; ++i)
			{
				var effectsProperty = this[i];
				effectsProperty.MaxCount = int.MaxValue;
				effectsProperty.IsStatic = false;
			}
#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty(this);
#endif
		}
		[ContextMenu("SetAllMaxTo1")]
		private void EditorSetMaxTo1()
		{
			for (int i = 0; i < this.Count; ++i)
			{
				var effectsProperty = this[i];
				effectsProperty.MaxCount = 1;
			}
#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty(this);
#endif
		}
		[ContextMenu("ClearAllStatic")]
		private void EditorClearAllStatic()
		{
			for (int i = 0; i < this.Count; ++i)
			{
				var effectsProperty = this[i];
				effectsProperty.IsStatic = false;
			}
#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty(this);
#endif
		}
	}
}