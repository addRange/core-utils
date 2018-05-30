//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (1/13/2016)
//----------------------------------------------------------------------------------------------

using System;
using UnityEngine;

namespace Effects.Player.Particle
{
	[Serializable, RequireComponent(typeof(ParticleSystem))]
	public class ParticlePlayer : EffectPlayer
	{
		public override void Play(Transform parent, Vector3 pos, Quaternion rot)
		{
			base.Play(parent, pos, rot);
			Particles.Play(false);
		}

		public override void Stop()
		{
			Particles.Stop();
			Particles.Clear();
			base.Stop();
		}

		public override void Fade()
		{
			Particles.Stop();
			base.Fade();
		}

		[ContextMenu("ShowInfo")]
		public void ShowInfo()
		{
			Debug.Log("Particles.isPlaying=" + Particles.isPlaying
				+ "\r\n" + "Particles.time=" + Particles.time
				+ "\r\n" + "Particles.duration=" + Particles.main.duration
				+ "\r\n" + "Particles.playbackSpeed=" + Particles.main.simulationSpeed
				+ "\r\n" + "Particles.particleCount=" + Particles.particleCount
				, this);
		}

		public override bool IsPlaying
		{
			get { return Particles.isPlaying; }
			protected set { base.IsPlaying = value; }
		}

		public ParticleSystem Particles
		{
			get
			{
				if (m_particles == null)
				{
					m_particles = GetComponent<ParticleSystem>();
				}
				return m_particles;
			}
		}

		public ParticleSystemRenderer ParticleSystemSystemRenderer
		{
			get
			{
				if (m_particleSystemRenderer == null)
				{
					m_particleSystemRenderer = GetComponent<ParticleSystemRenderer>();
				}
				return m_particleSystemRenderer;
			}
		}

		private ParticleSystem m_particles = null;
		private ParticleSystemRenderer m_particleSystemRenderer = null;
	}
}