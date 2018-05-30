//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (2017.02.21)
//----------------------------------------------------------------------------------------------

using System;
using UnityEngine;
using Assert = Core.Utils.Assert;

namespace Effects.Player.Animation
{
	[Serializable, RequireComponent(typeof(UnityEngine.Animation))]
	public class AnimationPlayer : EffectPlayer
	{
		public override void Play(Transform parent, Vector3 pos, Quaternion rot)
		{
			base.Play(parent, pos, rot);
			Animation.Play();
		}

		public override void Stop()
		{
			Animation.Stop();
			base.Stop();
		}

		public override void Fade()
		{
			Animation.Stop();
			base.Fade();
		}

		public override bool IsPlaying
		{
			get { return Animation.isPlaying; }
			protected set { base.IsPlaying = value; }
		}

		public UnityEngine.Animation Animation
		{
			get
			{
				if (m_animation == null)
				{
					Assert.IsTrue(this != null, "Null " + this + " on " + name);
					Assert.IsTrue(gameObject != null, "Null gameobject on " + name);
					m_animation = GetComponent<UnityEngine.Animation>();
				}
				return m_animation;
			}
		}

		private UnityEngine.Animation m_animation = null;
	}
}