//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (1/13/2016)
//----------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Effects.Player.Audio
{
	[Serializable]
	[RequireComponent(typeof(AudioSource))]
	public class AudioPlayer : EffectPlayer
	{
		protected void Awake() { m_startVolume = Volume; }

		public override void Play(Transform parent, Vector3 pos, Quaternion rot)
		{
			base.Play(parent, pos, rot);
			Volume = m_startVolume;
			m_audioSource.Play();
			//if (CheckDebug(parent)) { Debug.Log(GetInfo("Play "), this); }
		}

		private bool CheckDebug(Transform parent)
		{
			if (!name.StartsWith("MatchGatesAndForm"))
			{
				return false;
			}

			return true;
		}

		public override void Stop()
		{
			//if (CheckDebug(transform.parent)) { Debug.Log(GetInfo("Stop "), this); }
			if (m_fadeCoroutine != null)
			{
				StopCoroutine(m_fadeCoroutine);
			}

			m_audioSource.Stop();
			base.Stop();
		}

		[ContextMenu("GetInfo")]
		private void GetInfo() { Debug.Log(GetInfo("GetInfo "), this); }

		private string GetInfo(string suffix)
		{
			List<string> parameters = new List<string>
			{
				(m_audioSource.isActiveAndEnabled ? "isActiveAndEnabled" : "not act/en"),
				(m_audioSource.isPlaying ? "IsPlaying" : "not playing"),
				("Time = " + m_audioSource.time),
				(m_audioSource.isVirtual ? "isVirtual" : "not isVirtual"),
				(m_audioSource.ignoreListenerPause ? "ignoreListenerPause" : "not ignoreListenerPause"),
				(m_audioSource.mute ? "mute" : "not mute"),
#if UNITY_EDITOR
				(UnityEditor.EditorApplication.isPaused ? "playerIsPaused" : "not playerIsPaused"),
#endif
				(AudioListener.pause ? "pause" : "not pause"),
				this.ToString()
			};
			if (m_audioSource.clip != null)
			{
				//float clipLoudness = -1f;
				//if (m_audioSource.clip.loadType == AudioClipLoadType.DecompressOnLoad)
				//{
				//	clipLoudness = 0f;
				//	int offsetSamples = Mathf.Min(m_audioSource.timeSamples, m_audioSource.clip.samples - m_sampleData.Length);
				//	m_audioSource.clip.GetData(m_sampleData, offsetSamples);
				//	foreach (var sample in m_sampleData)
				//	{
				//		clipLoudness += Mathf.Abs(sample);
				//	}
				//	clipLoudness /= m_sampleData.Length; //clipLoudness is what you are looking for
				//}

				parameters.InsertRange(
					0, new string[]
					{
						"Name: " + m_audioSource.clip.name,
						"LoadState: " + m_audioSource.clip.loadState.ToString(),
						"time: " + m_audioSource.time + "/" + m_audioSource.clip.length,
						"samples: " + m_audioSource.timeSamples + "/" + m_audioSource.clip.samples,
						//"clipLoudness: " + clipLoudness,
					});
			}
			else
			{
				parameters.Insert(0, "Null clip");
			}

			return suffix + string.Join(";\r\n", parameters.ToArray());
		}

		public override void SetState(EffectType et, bool state)
		{
			base.SetState(et, state);
			if (m_soundType == et)
			{
				m_audioSource.mute = !state;
			}
		}

		public override void Fade()
		{
			base.Fade();
			Fade(m_fadeTime, 0, true);
		}

		/// <summary>
		/// Fade to target volume
		/// </summary>
		/// <param name="fadeTime"></param>
		/// <param name="toVolume"></param>
		/// <param name="withStop">Mean that after fade need stop this sound effect</param>
		public void Fade(float fadeTime, float toVolume, bool withStop = false)
		{
			if (m_fadeCoroutine != null)
			{
				StopCoroutine(m_fadeCoroutine);
			}

			if (!gameObject.activeSelf)
			{
				Debug.Break();
			}

			m_fadeCoroutine = StartCoroutine(FadeCoroutine(fadeTime, toVolume, withStop));
		}

		private IEnumerator FadeCoroutine(float fadeTime, float toVolume, bool withStop)
		{
			//Debug.Log("Start Fade " + Volume + " => " + toVolume + "; " + fadeTime + "; " + withStop + "; " + this, this);
			float fromVolume = Volume;
			//if (CheckDebug(transform.parent)) { Debug.Log(GetInfo("Start FadeCoroutine " + fromVolume + " => " + toVolume), this); }
			float startTime = Time.time;
			if (Math.Abs(fromVolume - toVolume) > 0.01f)
			{
				while (true)
				{
					float timeFromStart = Mathf.Min(Time.time - startTime, fadeTime);
					float progress = timeFromStart / fadeTime;
					Volume = Mathf.Lerp(fromVolume, toVolume, progress);
					//Debug.Log(progress + "; time=" + (Time.time - startTime) + "; timeFromStart=" + timeFromStart);
					if (progress >= 1)
					{
						break;
					}

					// if need - can return progress, but it was boxing
					yield return null;
				}
			}

			//Debug.Log("End Fade " + Volume + " => " + toVolume + "; " + fadeTime + "; " + withStop + "; " + this, this);
			Volume = toVolume;
			if (withStop)
			{
				m_audioSource.Stop();
			}

			//if (CheckDebug(transform.parent)) { Debug.Log(GetInfo("End FadeCoroutine " + fromVolume + " => " + toVolume), this); }
		}

		protected override void OnValidate()
		{
			base.OnValidate();
			if (m_audioSource == null)
			{
				m_audioSource = GetComponent<AudioSource>();
			}
		}

		public float Volume
		{
			get { return m_audioSource.volume; }
			set { m_audioSource.volume = value; }
		}

		public float Length
		{
			get { return m_audioSource.clip.length; }
		}

		public float RemainLength
		{
			get { return m_audioSource.clip.length - m_audioSource.time; }
		}

		public override bool IsPlaying
		{
			get
			{
				if (m_audioSource.isActiveAndEnabled)
				{
					if (m_audioSource.clip != null && m_audioSource.clip.loadState == AudioDataLoadState.Loading)
					{
						return true;
					}
					// HACK for Unity 5.5.0
#if UNITY_EDITOR
					if (UnityEditor.EditorApplication.isPaused)
					{
						return true;
					}
#endif
				}

				return m_audioSource.isPlaying;
			}
		}

		public float FadeTime
		{
			get { return m_fadeTime; }
		}

		public float StartVolume
		{
			get { return m_startVolume; }
		}

		[SerializeField]
		private AudioSource m_audioSource;

		[SerializeField]
		private EffectType m_soundType = EffectType.Sound;

		[SerializeField]
		private float m_fadeTime = 1.5f;

		private float m_startVolume = 1;

		private Coroutine m_fadeCoroutine = null;
		//private float[] m_sampleData = new float[1024];
	}
}