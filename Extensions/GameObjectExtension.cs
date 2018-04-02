//----------------------------------------------------------------------------------------------
// Created by Anton [Umka] Ushkalov (2016.12.28)
//----------------------------------------------------------------------------------------------

using UnityEngine;

namespace Core.Utils
{
	public static class GameObjectExtension
	{
		public static void TrySetActive(this GameObject _this, bool active)
		{
			if (_this.activeSelf != active)
			{
				_this.SetActive(active);
			}
		}
	}
}