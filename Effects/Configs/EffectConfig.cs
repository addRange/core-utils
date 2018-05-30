//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (2/17/2016)
//----------------------------------------------------------------------------------------------

using UnityEngine;
using Assert = Core.Utils.Assert;

namespace Effects.Configs
{
	using Player;

	[System.Serializable]
	public class EffectConfig
	{
		public EffectConfig() { }

		public EffectConfig(string effectName = null, Transform parent = null)
		{
			m_effectName = effectName;
			ConfigParent = parent;
		}

		public EffectPlayer PlayEffectInPos()
		{
			if (string.IsNullOrEmpty(m_effectName))
			{
				return null;
			}

			if (string.IsNullOrEmpty(m_suffixedName))
			{
				m_suffixedName = m_effectName + m_curSuffix;
			}

			StopEffect();
			CurrentEffectPlayer = EffectManager.Instance.PlayEffect(
				m_suffixedName, GamePlace.Game, null, ConfigParent.position,
				ConfigParent.rotation, 0);
			return CurrentEffectPlayer;
		}

		public EffectPlayer PlayEffect()
		{
			if (string.IsNullOrEmpty(m_effectName))
			{
				return null;
			}

			if (string.IsNullOrEmpty(m_suffixedName))
			{
				m_suffixedName = m_effectName + m_curSuffix;
			}

			StopEffect();
			CurrentEffectPlayer = EffectManager.Instance.PlayEffect(m_suffixedName, GamePlace.Game, ConfigParent);
			return CurrentEffectPlayer;
		}

		public void StopEffect()
		{
			if (CurrentEffectPlayer == null)
			{
				return;
			}

			CurrentEffectPlayer.Stop();
			CurrentEffectPlayer = null;
		}

		public void FadeEffect()
		{
			if (CurrentEffectPlayer == null)
			{
				return;
			}

			CurrentEffectPlayer.Fade();
			CurrentEffectPlayer = null;
		}

		public void SetSuffix(string suffix)
		{
			if (m_curSuffix == suffix)
			{
				return;
			}

			m_curSuffix = suffix;
			if (string.IsNullOrEmpty(m_effectName))
			{
				return;
			}

			m_suffixedName = m_effectName + m_curSuffix;
		}

		private void OnCurrentEffectStoped(EffectPlayer effectPlayer)
		{
			Assert.AreEqual(effectPlayer, CurrentEffectPlayer, "CurrentEffectPlayer != effectPlayer");
			CurrentEffectPlayer = null;
		}

		public EffectPlayer CurrentEffectPlayer
		{
			get { return m_currentEffectPlayer; }
			private set
			{
				if (m_currentEffectPlayer != null)
				{
					m_currentEffectPlayer.EventBeforeStop -= OnCurrentEffectStoped;
				}

				m_currentEffectPlayer = value;
				if (m_currentEffectPlayer != null)
				{
					m_currentEffectPlayer.EventBeforeStop += OnCurrentEffectStoped;
				}
			}
		}

		public bool IsPlaying
		{
			get
			{
				if (CurrentEffectPlayer == null)
				{
					return false;
				}

				return CurrentEffectPlayer.IsPlaying;
			}
		}

		public Transform ConfigParent
		{
			get { return m_parent; }
			set { m_parent = value; }
		}

		[SerializeField, EffectName]
		public string m_effectName = string.Empty;

		[SerializeField]
		private Transform m_parent = null;

		private string m_suffixedName = null;
		private string m_curSuffix = string.Empty;
		private EffectPlayer m_currentEffectPlayer = null;
	}
}