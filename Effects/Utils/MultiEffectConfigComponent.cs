//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (2018.07.12)
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Effects.Player.Utils
{
	using Configs;

	[Serializable]
	public class MultiEffectConfigComponent : MonoBehaviour
	{
		public EffectPlayer PlayEffectInPos()
		{
			return m_effects.PlayEffectInPos();
		}
		public EffectPlayer PlayEffect()
		{
			return m_effects.PlayEffect();
		}

		public void StopEffect()
		{
			m_effects.StopEffect();
		}

		public void FadeEffect()
		{
			m_effects.FadeEffect();
		}

		public EffectPlayer CurrentEffectPlayer
		{
			get
			{
				return m_effects.CurrentEffectPlayer;
			}
		}

		public Func<List<EffectConfig>, EffectConfig> EffectSelector
		{
			get { return m_effects.EffectSelector; }
			set { m_effects.EffectSelector = value; }
		}

		[SerializeField]
		private MultiEffectConfig m_effects = new MultiEffectConfig();
	}
}