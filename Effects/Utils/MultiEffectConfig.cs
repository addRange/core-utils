//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (4/27/2016)
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Effects.Player.Utils
{
	using Configs;

	[Serializable]
	public class MultiEffectConfig
	{
		public EffectPlayer PlayEffectInPos() { return EffectSelector(m_effects).PlayEffect(); }
		public EffectPlayer PlayEffect() { return EffectSelector(m_effects).PlayEffectInPos(); }

		public void StopEffect()
		{
			for (int i = 0; i < m_effects.Count; i++)
			{
				var effectConfig = m_effects[i];
				effectConfig.StopEffect();
			}
		}

		public void FadeEffect()
		{
			for (int i = 0; i < m_effects.Count; i++)
			{
				var effectConfig = m_effects[i];
				if (effectConfig.CurrentEffectPlayer != null)
				{
					effectConfig.FadeEffect();
				}
			}
		}

		// TODO: need use EventBeforeStop
		public EffectPlayer CurrentEffectPlayer
		{
			get
			{
				for (int i = 0; i < m_effects.Count; i++)
				{
					var effectConfig = m_effects[i];
					if (effectConfig.CurrentEffectPlayer != null)
					{
						return effectConfig.CurrentEffectPlayer;
					}
				}

				return null;
			}
		}

		public Func<List<EffectConfig>, EffectConfig> EffectSelector
		{
			get
			{
				if (m_curEffectSelector != null)
				{
					return m_curEffectSelector;
				}

				return DefaulSelector;
			}
			set { m_curEffectSelector = value; }
		}

		public static int DefaulSelectorIndex(List<EffectConfig> effects) { return Random.Range(0, effects.Count); }

		public static EffectConfig DefaulSelector(List<EffectConfig> effects)
		{
			return effects[DefaulSelectorIndex(effects)];
		}

		[SerializeField]
		private List<EffectConfig> m_effects = new List<EffectConfig>();

		private Func<List<EffectConfig>, EffectConfig> m_curEffectSelector = null;
	}
}