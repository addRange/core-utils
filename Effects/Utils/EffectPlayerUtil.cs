//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (2/17/2016)
//----------------------------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace Effects.Player.Utils
{
	using Configs;

	public class EffectPlayerUtil : MonoBehaviour
	{
		public void PlayEffectInPos(int indexOfEffect)
		{
			var effectForPlay = m_effects[indexOfEffect];
			/*var player = */
			EffectManager.Instance.PlayEffect(
				effectForPlay.m_effectName, GamePlace.Game, null,
				effectForPlay.ConfigParent.position, Quaternion.identity, 0);
			//Debug.Log("Play " + player, player);
		}

		public void PlayEffectInParent(int indexOfEffect)
		{
			var effectForPlay = m_effects[indexOfEffect];
			EffectManager.Instance.PlayEffect(effectForPlay.m_effectName, GamePlace.Game, effectForPlay.ConfigParent);
		}

		[SerializeField]
		private List<EffectConfig> m_effects = new List<EffectConfig>();
	}
}