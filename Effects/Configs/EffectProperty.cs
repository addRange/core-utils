//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (1/13/2016)
//----------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using UnityEngine;

namespace Effects
{
	[Serializable]
	public class EffectProperty
	{
		public EffectProperty() { }
		public EffectProperty(string effectName, string effectPath, GamePlace gamePlace, int maxCount = 1, PriorityType priority = PriorityType.Time)
		{
			m_effectName = effectName;
			m_effectPath = effectPath;
			m_gamePlace = gamePlace;
			m_maxCount = maxCount;
			m_priority = priority;

			FixAssetPath();
		}

		public bool IsStatic { get { return m_isStatic; } set { m_isStatic = value; } }
		public GamePlace GamePlace { get { return m_gamePlace; } }
		public string EffectName { get { return m_effectName; } }

		public string EffectPath
		{
			get
			{
				if (string.IsNullOrEmpty(m_effectPath))
				{
					return m_effectName;
				}
				return m_effectPath;
			}
		}

		public PriorityType PriorityType { get { return m_priority; } }
		public int MaxCount
		{
			get { return m_maxCount; }
			set { m_maxCount = value; }
		}

		[Conditional("UNITY_EDITOR")]
		public void SetEditorEffectPath(string effectPath)
		{
			m_effectPath = effectPath;
			FixAssetPath();
		}

		[Conditional("UNITY_EDITOR")]
		private void FixAssetPath()
		{
			if (m_effectName == m_effectPath || string.IsNullOrEmpty(m_effectPath))
			{
				m_effectPath = string.Empty;
			}
		}

		[Conditional("UNITY_EDITOR"), ContextMenu("Validate")]
		private void OnValidate()
		{
			if (m_isStatic && m_staticCount == 0)
			{
				m_staticCount = m_maxCount;
				UnityEngine.Debug.Log("Auto set static count for static effect to max '" + m_effectName + "'; " + m_maxCount);
			}
		}

		[SerializeField]
		private string m_effectName = null;
		[SerializeField]
		private string m_effectPath = null;
		[SerializeField]
		private bool m_isStatic = false;
		[SerializeField]
		private int m_staticCount = 0;
		[SerializeField]
		private GamePlace m_gamePlace = GamePlace.Game;
		[SerializeField]
		private PriorityType m_priority = PriorityType.Time;
		[SerializeField]
		private int m_maxCount = 3;
	}
}