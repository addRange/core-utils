//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (1/13/2016)
//----------------------------------------------------------------------------------------------

using System;
using UnityEngine;

namespace Effects.Player
{
	[Serializable]
	public abstract class EffectPlayer : MonoBehaviour
	{
		public Action<EffectPlayer> EventBeforeStop { get; set; }

		public virtual void Play(Transform parent, Vector3 pos, Quaternion rot)
		{
			IsPlaying = true;
			if (!DontChangeTransform)
			{
				if (transform.parent != parent)
				{
					transform.SetParent(parent, false);
				}

				if (transform.localPosition != pos)
				{
					transform.localPosition = pos;
				}

				if (transform.localRotation != rot)
				{
					transform.localRotation = rot;
				}
			}

			gameObject.SetActive(true);
		}

		public virtual void Stop()
		{
			EventBeforeStop.SafeInvoke(this);
			gameObject.SetActive(false);
			if (!DontChangeTransform)
			{
				transform.SetParent(EffectManager.Instance.transform, false);
			}

			IsPlaying = false;
		}

		public virtual void Fade() { }

		public virtual void SetState(EffectType et, bool state) { }

		[ContextMenu("Validate")]
		protected virtual void OnValidate() { }

		/// <summary>
		/// Mean not change on start/Stop transform pos/rot/parent
		/// </summary>
		public bool DontChangeTransform { get; set; }

		public virtual bool IsPlaying { get; protected set; }
	}
}