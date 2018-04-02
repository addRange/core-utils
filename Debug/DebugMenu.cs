//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (2017.02.02)
//----------------------------------------------------------------------------------------------

using UnityEngine;

namespace DebugMenu
{
	public abstract class DebugMenu<T> : SingletonGameObject<T> where T : DebugMenu<T>
	{
		protected override void Init()
		{
			base.Init();
			gameObject.SetActive(m_enabled);
		}

		protected override void DeInit()
		{
			base.DeInit();
		}

		[SerializeField]
		private bool m_enabled = false;
	}
}