//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (2016.10.20)
//----------------------------------------------------------------------------------------------

using System;
using UnityEngine;

namespace Effects.Camera
{
	[Serializable]
	public abstract class ChangerByTimeScale : MonoBehaviour
	{
		private void Update()
		{
			if (Math.Abs(m_lastApplyedScale - Time.timeScale) > Tolerance)
			{
				if (m_maxApplySpeed > 0)
				{
					m_lastApplyedScale = Mathf.Lerp(m_lastApplyedScale, Time.timeScale, Time.deltaTime * m_maxApplySpeed);
				}

				if (m_maxApplySpeed <= 0 || Math.Abs(m_lastApplyedScale - Time.timeScale) < Tolerance)
				{
					m_lastApplyedScale = Time.timeScale;
				}

				float newVal = m_byScaleChangeParams.Evaluate(m_lastApplyedScale);
				ApplyNewValue(newVal);
			}
		}

		protected abstract void ApplyNewValue(float newVal);

		private const float Tolerance = 0.01f;

		[SerializeField]
		private AnimationCurve m_byScaleChangeParams = new AnimationCurve();

		[SerializeField]
		private float m_maxApplySpeed = 3;

		private float m_lastApplyedScale = -1;
	}
}