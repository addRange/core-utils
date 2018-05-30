//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (1/13/2016)
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;
using Assert = Core.Utils.Assert;

namespace Effects.Player
{
	[Serializable]
	public class CompositePlayer : EffectPlayer
	{
		public override void Play(Transform parent, Vector3 pos, Quaternion rot)
		{
			base.Play(parent, pos, rot);
			for (int i = 0; i < m_effects.Count; i++)
			{
				m_effects[i].DontChangeTransform = true;
				m_effects[i].Play(null, Vector3.zero, Quaternion.identity);
			}
		}

		public override void Stop()
		{
			for (int i = 0; i < m_effects.Count; i++)
			{
				m_effects[i].DontChangeTransform = true;
				m_effects[i].Stop();
			}

			base.Stop();
		}

		protected override void OnValidate()
		{
			base.OnValidate();
			// Use unityEngine for assert in any InEditor case
			Assert.IsTrue(m_effects.Count > 0, "not found effects");
		}

		public override void Fade()
		{
			base.Fade();
			for (int i = 0; i < m_effects.Count; i++)
			{
				m_effects[i].Fade();
			}
		}

		public override void SetState(EffectType et, bool state)
		{
			base.SetState(et, state);
			for (int i = 0; i < m_effects.Count; i++)
			{
				m_effects[i].SetState(et, state);
			}
		}

		public override bool IsPlaying
		{
			get
			{
				if (m_effects.Count == 0)
				{
					return false;
				}

				for (int i = 0; i < m_effects.Count; i++)
				{
					if (m_effects[i].IsPlaying)
					{
						return true;
					}
				}

				return false;
			}
		}

		[SerializeField]
		private List<EffectPlayer> m_effects = new List<EffectPlayer>();
	}
}