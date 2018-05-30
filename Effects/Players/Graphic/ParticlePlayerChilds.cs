//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (1/13/2016)
//----------------------------------------------------------------------------------------------

using System;
using Effects.Particle.ParamChangers;
using UnityEngine;

namespace Effects.Player.Particle
{
	[Serializable]
	public class ParticlePlayerChilds : EffectPlayer
	{
		public override void Play(Transform parent, Vector3 pos, Quaternion rot)
		{
			base.Play(parent, pos, rot);
			for (int i = 0; i < m_particles.Length; ++i)
			{
				m_particles[i].Play(false);
			}			
		}

		public override void Stop()
		{
			for (int i = 0; i < m_particles.Length; ++i)
			{
				m_particles[i].Stop();
				m_particles[i].Clear();
			}
			base.Stop();
		}

		public override void Fade()
		{
			for (int i = 0; i < m_particles.Length; ++i)
			{
				m_particles[i].Stop();
			}
			base.Fade();
		}

		public override bool IsPlaying
		{
			get
			{
				for (int i = 0; i < m_particles.Length; ++i)
				{
					if (m_particles[i].isPlaying)
					{
						return true;
					}
				}
				return false;
			}
		}

		public ParticleSystem[] Particles
		{
			get { return m_particles; }
		}

		public ParticleSystemRenderer[] ParticleSystemRenderers
		{
			get
			{
				if (m_particleSystemRenderers == null)
				{
					m_particleSystemRenderers = new ParticleSystemRenderer[m_particles.Length];
					for (int i = 0; i < m_particles.Length; i++)
					{
						m_particleSystemRenderers[i] = m_particles[i].GetComponent<ParticleSystemRenderer>();
					}
				}
				return m_particleSystemRenderers;
			}
		}

		public ParticleSystem.ShapeModule[] ParticleSystemShapeModules
		{
			get
			{
				if (m_particleSystemShapeModules == null)
				{
					m_particleSystemShapeModules = new ParticleSystem.ShapeModule[m_particles.Length];
					for (int i = 0; i < m_particles.Length; i++)
					{
						m_particleSystemShapeModules[i] = m_particles[i].shape;
					}
				}
				return m_particleSystemShapeModules;
			}
		}

		public ParticleParamChanger[][] ParticleParamChangers
		{
			get
			{
				if (m_particleParamChangers == null)
				{
					m_particleParamChangers = new ParticleParamChanger[m_particles.Length][];
					for (int i = 0; i < m_particles.Length; i++)
					{
						var curChangers = m_particles[i].GetComponents<ParticleParamChanger>();
						m_particleParamChangers[i] = new ParticleParamChanger[curChangers.Length];
						for (int j = 0; j < curChangers.Length; j++)
						{
							var particleParamChanger = curChangers[j];
							m_particleParamChangers[i][j] = particleParamChanger;
						}
					}
				}
				return m_particleParamChangers;
			}
		}
		
		[SerializeField]
		private ParticleSystem[] m_particles = new ParticleSystem[0];

		private ParticleSystemRenderer[] m_particleSystemRenderers = null;
		private ParticleSystem.ShapeModule[] m_particleSystemShapeModules = null;
		private ParticleParamChanger[][] m_particleParamChangers = null;
	}
}