//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (2016.12.16)
//----------------------------------------------------------------------------------------------

using UnityEngine;

namespace Effects.Particle.ParamChangers
{
	public abstract class ParticleParamChanger : MonoBehaviour
	{
		public float Koefficient
		{
			get { return m_koefficient; }
			set
			{
				m_koefficient = value;
				SetValue(m_curve.Evaluate(m_koefficient));
			}
		}

		protected abstract void SetValue(float value);

		public ParticleSystem ParticleSystem { get { return m_particleSystem; } }

		[SerializeField]
		private ParticleSystem m_particleSystem;
		[SerializeField]
		private AnimationCurve m_curve = new AnimationCurve();

		private float m_koefficient = 1;
	}
}