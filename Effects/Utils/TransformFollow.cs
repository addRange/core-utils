//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (2017.01.13)
//----------------------------------------------------------------------------------------------

using System;
using UnityEngine;

namespace Effects.Player.Utils
{
	[Serializable, RequireComponent(typeof(EffectPlayer))]
	public class TransformFollow : MonoBehaviour
	{
		private void OnEnable()
		{
			m_currentTransform = transform.parent;
			transform.SetParent(null);
		}

		private void OnDisable() { m_currentTransform = null; }

		private void Update()
		{
			if (m_currentTransform != null)
			{
				transform.position = m_currentTransform.position;
			}
		}

		private Transform m_currentTransform = null;
	}
}