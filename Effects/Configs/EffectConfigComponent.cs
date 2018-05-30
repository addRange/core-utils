//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (2017.02.22)
//----------------------------------------------------------------------------------------------

using UnityEngine;

namespace Effects.Configs
{
	[System.Serializable]
	public class EffectConfigComponent : MonoBehaviour
	{
		public void SetEffectName(string targetEffectName) { m_effect = new EffectConfig(targetEffectName, transform); }

		public void PlayInParent() { m_effect.PlayEffect(); }

		public void PlayInPos() { m_effect.PlayEffectInPos(); }

		[SerializeField]
		private EffectConfig m_effect = new EffectConfig();
	}
}