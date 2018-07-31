//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (2018.07.12)
//----------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Effects.Player.Audio
{
	[Serializable]
	public class RandomAudioPlayer : EffectPlayer
	{
		protected void Awake()
		{
			m_startVolumes = new float[m_audioSources.Length];
			for (var i = 0; i < m_audioSources.Length; i++)
			{
				m_startVolumes[i] = m_audioSources[i].volume;
			}

			m_curAudioSourceIndex = Random.Range(0, m_audioSources.Length);
			m_curAudioSource = m_audioSources[m_curAudioSourceIndex];
		}

		public override void Play(Transform parent, Vector3 pos, Quaternion rot)
		{
			base.Play(parent, pos, rot);
			m_curAudioSourceIndex = Random.Range(0, m_audioSources.Length);
			m_curAudioSource = m_audioSources[m_curAudioSourceIndex];
			Volume = m_startVolumes[m_curAudioSourceIndex];
			m_curAudioSource.Play();
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

			m_curAudioSource.Stop();
			base.Stop();
		}

		[ContextMenu("GetInfo")]
		private void GetInfo() { Debug.Log(GetInfo("GetInfo "), this); }

		private string GetInfo(string suffix)
		{
			List<string> parameters = new List<string>
			{
				(m_curAudioSource.isActiveAndEnabled ? "isActiveAndEnabled" : "not act/en"),
				(m_curAudioSource.isPlaying ? "IsPlaying" : "not playing"),
				("Time = " + m_curAudioSource.time),
				(m_curAudioSource.isVirtual ? "isVirtual" : "not isVirtual"),
				(m_curAudioSource.ignoreListenerPause ? "ignoreListenerPause" : "not ignoreListenerPause"),
				(m_curAudioSource.mute ? "mute" : "not mute"),
#if UNITY_EDITOR
				(UnityEditor.EditorApplication.isPaused ? "playerIsPaused" : "not playerIsPaused"),
#endif
				(AudioListener.pause ? "pause" : "not pause"),
				this.ToString()
			};
			if (m_curAudioSource.clip != null)
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
						"Name: " + m_curAudioSource.clip.name,
						"LoadState: " + m_curAudioSource.clip.loadState.ToString(),
						"time: " + m_curAudioSource.time + "/" + m_curAudioSource.clip.length,
						"samples: " + m_curAudioSource.timeSamples + "/" + m_curAudioSource.clip.samples,
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
				foreach (var audioSource in m_audioSources)
				{
					audioSource.mute = !state;
				}
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
			float startTime = GetTime();
			if (Math.Abs(fromVolume - toVolume) > 0.01f)
			{
				while (true)
				{
					float timeFromStart = Mathf.Min(GetTime() - startTime, fadeTime);
					float progress = timeFromStart / fadeTime;
					Volume = Mathf.Lerp(fromVolume, toVolume, progress);
					//Debug.Log(progress + "; time=" + (GetTime() - startTime) + "; timeFromStart=" + timeFromStart);
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
				m_curAudioSource.Stop();
			}

			//if (CheckDebug(transform.parent)) { Debug.Log(GetInfo("End FadeCoroutine " + fromVolume + " => " + toVolume), this); }
		}

		private float GetTime()
		{
			if (m_fadeRealTime)
			{
				return Time.realtimeSinceStartup;
			}
			return Time.time;
		}

		protected override void OnValidate()
		{
			base.OnValidate();
			if (m_curAudioSource == null)
			{
				m_curAudioSource = GetComponent<AudioSource>();
			}
		}

		public float Volume
		{
			get { return m_curAudioSource.volume; }
			set { m_curAudioSource.volume = value; }
		}

		public float Length
		{
			get { return m_curAudioSource.clip.length; }
		}

		public float RemainLength
		{
			get { return m_curAudioSource.clip.length - m_curAudioSource.time; }
		}

		public override bool IsPlaying
		{
			get
			{
				if (m_curAudioSource.isActiveAndEnabled)
				{
					if (m_curAudioSource.clip != null && m_curAudioSource.clip.loadState == AudioDataLoadState.Loading)
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

				return m_curAudioSource.isPlaying;
			}
		}

		public float FadeTime
		{
			get { return m_fadeTime; }
		}

		public float StartVolume
		{
			get { return m_startVolumes[m_curAudioSourceIndex]; }
		}

		[SerializeField]
		private AudioSource[] m_audioSources = new AudioSource[0];

		[SerializeField]
		private EffectType m_soundType = EffectType.Sound;
		
		[SerializeField]
		private float m_fadeTime = 1.5f;
		[SerializeField]
		private bool m_fadeRealTime = false;

		private float[] m_startVolumes = null;

		private int m_curAudioSourceIndex = -1;
		private AudioSource m_curAudioSource;
		private Coroutine m_fadeCoroutine = null;
		//private float[] m_sampleData = new float[1024];
	}
}