//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (2018.07.10)
//----------------------------------------------------------------------------------------------

using UnityEngine;

namespace Effects.Player.Utils
{
	public class AnimationPlayerUtil : MonoBehaviour
	{
		public void PlayCurAnim() { m_animation.Play(); }

		public void PlayAnim(string animName) { m_animation.Play(animName); }

		[SerializeField]
		private UnityEngine.Animation m_animation = null;
	}
}