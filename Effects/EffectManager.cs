//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (1/10/2016)
//----------------------------------------------------------------------------------------------

// later need write editor for autoConfigurates statics and max play count
// mode of collecting EffectInfo statistic
//#define COLLECT_STATS_MODE

using System;
using System.Collections.Generic;
using UnityEngine;
using Assert = Core.Utils.Assert;
using UnityEngine.Profiling;
#if COLLECT_STATS_MODE
using System.Text;
#endif
#if UNITY_EDITOR
using System.Linq;
#endif

namespace Effects
{
	using Player;

	public class EffectManager : SingletonGameObject<EffectManager>
	{
		private class EffectInfo
		{
			public EffectInfo(GameObject gameObject)
			{
				GameObject = gameObject;
				EffectPlayer = GameObject.GetComponent<EffectPlayer>();
				Priority = 0;
				GameObject.SetActive(false);
			}

			public int Priority { get; set; }
			public float LastPlayTime { get; set; }

			public GameObject GameObject { get; private set; }
			public EffectPlayer EffectPlayer { get; private set; }
		}

		private class EffectsGroup
		{
			public EffectsGroup(EffectManager effectManager, EffectProperty effectsProperties)
			{
				m_effectManager = effectManager;
				EffectsProperties = effectsProperties;
				Effects = new List<EffectInfo>();

				string path = m_effectManager.GetGamePlaceEffectPath(effectsProperties.GamePlace);
				Resource = Resources.Load<GameObject>(path + effectsProperties.EffectPath);
				if (Resource == null)
				{
					effectsProperties.MaxCount = 0;
					Assert.IsTrue(false, "Can't load effect at '" + path + effectsProperties.EffectPath + "'");
				}
				else
				{
#if !UNITY_EDITOR
// disable resource for not enable/disable instantiated
				Resource.gameObject.SetActive(false);
#endif
				}
			}

			public EffectInfo GetEffectInfoForPlay(int priority, List<EffectPlayer> activeEffects)
			{
				EffectInfo best = Effects.Find(ei => !activeEffects.Contains(ei.EffectPlayer));
				if (best == null)
				{
					if (Effects.Count < EffectsProperties.MaxCount)
					{
						best = CreateEffectInfo();
					}
					else
					{
						var possibleEffects = Effects; //.FindAll(e => e.Priority <= priority);
						if (possibleEffects.Count > 0)
						{
							best = possibleEffects[0];
							switch (EffectsProperties.PriorityType)
							{
								case PriorityType.Time:
									for (int i = 1; i < possibleEffects.Count; i++)
									{
										if (best.LastPlayTime >= possibleEffects[i].LastPlayTime)
										{
											best = possibleEffects[i];
										}
									}

									break;
								case PriorityType.Priority:
									for (int i = 1; i < possibleEffects.Count; i++)
									{
										if (best.Priority >= possibleEffects[i].Priority)
										{
											best = possibleEffects[i];
										}
									}

									break;
								case PriorityType.Distance:
								case PriorityType.PriorityAndDist:
									Assert.IsTrue(false, "Not implemented " + EffectsProperties.PriorityType);
									break;
							}
						}

						/* TODO: Get best EffectInfo by PriorityType */
					}
				}

				if (best == null)
				{
					Assert.IsTrue(false, "Effect not found " + EffectsProperties.EffectName);
					return null;
				}

				best.Priority = priority;
				return best;
			}

			public void CreateMaxEffects()
			{
				for (int i = Effects.Count; i < EffectsProperties.MaxCount; ++i)
				{
					CreateEffectInfo();
				}
			}

			public void Unload()
			{
				for (int i = 0; i < Effects.Count; i++)
				{
					var effectInfo = Effects[i];
					DestroyImmediate(effectInfo.EffectPlayer.gameObject);
				}

				Effects.Clear();
				Resources.UnloadAsset(Resource);
				Resource = null;
			}

			private EffectInfo CreateEffectInfo()
			{
				// Not disable objects for call Awake on its
				var result = Instantiate(Resource, null, false);
				DontDestroyOnLoad(result);
				var effectInfo = new EffectInfo(result);
				Effects.Add(effectInfo);
				return effectInfo;
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="effectInfo"></param>
			/// <param name="parent"></param>
			/// <param name="pos"></param>
			/// <param name="rot"></param>
			/// <param name="effectPlayer"></param>
			/// <returns>True if terminated played effect</returns>
			public bool Play(EffectInfo effectInfo, Transform parent, Vector3 pos, Quaternion rot, out EffectPlayer effectPlayer)
			{
				bool terminated = false;
				effectPlayer = effectInfo.EffectPlayer;
				if (effectPlayer.isActiveAndEnabled && effectPlayer.IsPlaying)
				{
					terminated = true;
					effectPlayer.Stop();
				}

				effectInfo.LastPlayTime = Time.realtimeSinceStartup;
				effectPlayer.Play(parent, pos, rot);
				PlayCollectStats();
				return terminated;
			}

			[System.Diagnostics.Conditional("COLLECT_STATS_MODE")]
			private void PlayCollectStats()
			{
#if COLLECT_STATS_MODE
			PlayCount++;
			if (FirstPlayTime <= 0)
			{
				FirstPlayTime = Time.time;
			}
			MaxPlayAtSameTime = Mathf.Max(MaxPlayAtSameTime, Effects.FindAll(ef => ef.EffectPlayer.IsPlaying).Count);
#endif
			}

#if COLLECT_STATS_MODE
		public StringBuilder GetPrintCollectedStats(StringBuilder sb)
		{
			sb.Append(EffectsProperties.EffectName).
				Append(" (").Append(EffectsProperties.EffectPath).Append(")\t").
				Append("PlayCount=\t").Append(PlayCount).Append("\t").
				Append("FirstPlayTime=\t").Append(FirstPlayTime).Append("\t").
				Append("MaxPlayAtSameTime=\t").Append(MaxPlayAtSameTime);
			return sb;
		}

		private int PlayCount { get; set; }
		private float FirstPlayTime { get; set; }
		private int MaxPlayAtSameTime { get; set; }
#endif

			public List<EffectInfo> Effects { get; private set; }

			private GameObject Resource { get; set; }
			private EffectProperty EffectsProperties { get; set; }

			private EffectManager m_effectManager = null;
		}

		protected override void Init()
		{
			base.Init();
			m_effectGroups.Add(GamePlace.Game, new Dictionary<string, EffectsGroup>());
			m_effectGroups.Add(GamePlace.SubGame, new Dictionary<string, EffectsGroup>());

			Profiler.BeginSample("EffectManager.CreateStaticEffects()");
			CreateStaticEffects(Properties);
			Profiler.EndSample();

			foreach (var effType in Enum.GetValues(typeof(EffectType)))
			{
				m_curEffectStates.Add((EffectType)effType, true);
			}
		}

		protected override void DeInit()
		{
			UnloadEffects();
			base.DeInit();
		}

		private void UnloadEffects()
		{
			for (int i = 0; i < m_activeEffects.Count; i++)
			{
				var mActiveEffect = m_activeEffects[i];
#if UNITY_EDITOR
				Assert.IsNotNull(mActiveEffect, m_activeEffectNames[i]);
#endif
				mActiveEffect.Stop();
			}
#if COLLECT_STATS_MODE
		StringBuilder sb = new StringBuilder();
		foreach (var effectGroup in m_effectGroups)
		{
			foreach (var effectsGroup in effectGroup.Value)
			{
				effectsGroup.Value.GetPrintCollectedStats(sb);
				sb.AppendLine();
			}
		}
		Debug.Log(sb.ToString());
#endif
			m_activeEffects.Clear();
#if UNITY_EDITOR
			m_activeEffectNames.Clear();
#endif
			// TODO: unload effects resources
		}

		public void Update()
		{
			Profiler.BeginSample("EffectManager.Update", gameObject);
			if (m_isApplicationPaused)
			{
				Profiler.EndSample();
				return;
			}

			for (int i = 0; i < m_activeEffects.Count; ++i)
			{
				var activeEffectPlayer = m_activeEffects[i];
#if UNITY_EDITOR
				Assert.IsNotNull(activeEffectPlayer, m_activeEffectNames[i]);
#endif
				Profiler.BeginSample("ActiveEffect.IsPlaying", activeEffectPlayer);
				if (activeEffectPlayer.IsPlaying)
				{
					Profiler.EndSample();
					continue;
				}

				Profiler.EndSample();
				Profiler.BeginSample("ActiveEffect.Stop/Remove", activeEffectPlayer);
				activeEffectPlayer.Stop();
				Assert.IsFalse(activeEffectPlayer.DontChangeTransform, "DontChangeTransform seted on " + activeEffectPlayer);
				m_activeEffects.Remove(activeEffectPlayer);
#if UNITY_EDITOR
				m_activeEffectNames.RemoveAt(i);
#endif
				--i;
				Profiler.EndSample();
			}

			Profiler.EndSample();
		}

		public void CollectAllChildActiveEffects()
		{
			List<EffectPlayer> notDisabledEffects = new List<EffectPlayer>();
			for (int i = 0; i < m_activeEffects.Count; i++)
			{
				var e = m_activeEffects[i];
#if UNITY_EDITOR
				Assert.IsNotNull(e, m_activeEffectNames[i]);
#endif
				if (m_activeEffects[i].transform.parent == null)
				{
					notDisabledEffects.Add(m_activeEffects[i]);
					continue;
				}

				e.Stop();
				e.transform.SetParent(transform, false);
			}

			m_activeEffects.Clear();
			m_activeEffects.AddRange(notDisabledEffects);

#if UNITY_EDITOR
			m_activeEffectNames.Clear();
			m_activeEffectNames.AddRange(m_activeEffects.Select(e => e.name));
#endif
		}

		/// <summary>
		/// Create effect and start playing in parent
		/// </summary>
		/// <param name="id"></param>
		/// <param name="gamePlace"></param>
		/// <param name="parent"></param>
		/// <returns>Created effectPlayer or null if can`t play</returns>
		public EffectPlayer PlayEffect(
			string name, GamePlace gamePlace = GamePlace.Game, Transform parent = null, int priority = 0)
		{
			return PlayEffect(name, gamePlace, parent, Vector3.zero, Quaternion.identity, priority);
		}

		/// <summary>
		/// Create effect and start playing in parent and local pos of it
		/// </summary>
		/// <param name="name"></param>
		/// <param name="gamePlace"></param>
		/// <param name="parent"></param>
		/// <param name="pos"></param>
		/// <param name="rot"></param>
		/// <param name="priority"></param>
		/// <returns></returns>
		public EffectPlayer PlayEffect(
			string name, GamePlace gamePlace, Transform parent, Vector3 pos, Quaternion rot, int priority)
		{
			Profiler.BeginSample("EffectManager.PlayEffect()", gameObject);
			var group = GetEffectsGroup(name, gamePlace);
			// FOr disabled effects
			if (group == null)
			{
				Profiler.EndSample();
				return null;
			}

			EffectInfo effectInfo = group.GetEffectInfoForPlay(priority, m_activeEffects);
			if (effectInfo == null)
			{
				Profiler.EndSample();
				return null;
			}

			foreach (var curEffectState in m_curEffectStates)
			{
				effectInfo.EffectPlayer.SetState(curEffectState.Key, curEffectState.Value);
			}

			EffectPlayer player = null;
			bool isTerminated = group.Play(effectInfo, parent, pos, rot, out player);
			if (isTerminated)
			{
#if UNITY_EDITOR
				int index = m_activeEffects.IndexOf(player);
				m_activeEffectNames.RemoveAt(index);
#endif
				m_activeEffects.Remove(player);
			}

			m_activeEffects.Add(player);
#if UNITY_EDITOR
			m_activeEffectNames.Add(player.name);
#endif
			Profiler.EndSample();
			return player;
		}

		private EffectsGroup GetEffectsGroup(string effectName, GamePlace gamePlace)
		{
			if (!EffectEnabled(effectName))
			{
				return null;
			}

			var group = m_effectGroups[gamePlace];
			if (!group.ContainsKey(effectName))
			{
				var effectProp = GetEffectProp(effectName, gamePlace);
				var newGroup = new EffectsGroup(this, effectProp);
				group.Add(effectName, newGroup);
				return newGroup;
			}

			return group[effectName];
		}

		private EffectProperty GetEffectProp(string effectName, GamePlace gamePlace)
		{
			Profiler.BeginSample("m_effectProperties.Find");
			// OPTIMIZE: Create Dictionary for m_effectProperties ??
			var effectProp = Properties.Find(ep => ep.EffectName == effectName && ep.GamePlace == gamePlace);
			Profiler.EndSample();
			if (effectProp == null)
			{
				effectProp = new EffectProperty(effectName, effectName, gamePlace);
			}

			return effectProp;
		}

		public string GetGamePlaceEffectPath(GamePlace gamePlace)
		{
			switch (gamePlace)
			{
				case GamePlace.SubGame:
					return m_minigamePath;
				case GamePlace.Game:
					return m_path;
				default:
					Assert.IsTrue(false, "Wrong gamePlace=" + gamePlace);
					return m_path;
			}
		}

		public void LoadMinigame(string minigameName)
		{
			CurrentEffectsProperties = Resources.Load<EffectsProperties>(m_minigamePath + minigameName);
			Assert.IsNotNull(CurrentEffectsProperties, "Not found " + (m_minigamePath + minigameName));
			CreateStaticEffects(CurrentEffectsProperties);
		}

		public void UnloadMinigame()
		{
			var minigameGroup = m_effectGroups[GamePlace.SubGame];
			foreach (var effectsGroup in minigameGroup)
			{
				var info = effectsGroup.Value;
				Profiler.BeginSample("UnloadMinigame.FindActive");
				var curActive = m_activeEffects.FindAll(ae => info.Effects.Find(j => j.EffectPlayer == ae) != null);
#if UNITY_EDITOR
				foreach (var effectPlayer in curActive)
				{
					int index = m_activeEffects.IndexOf(effectPlayer);
					m_activeEffectNames.RemoveAt(index);
				}
#endif
				m_activeEffects.RemoveAll(ae => curActive.Contains(ae));
				Profiler.EndSample();
				for (int j = 0; j < curActive.Count; j++)
				{
					var effectPlayer = curActive[j];
					effectPlayer.Stop();
				}

				effectsGroup.Value.Unload();
			}

			minigameGroup.Clear();
			Resources.UnloadAsset(CurrentEffectsProperties);
			CurrentEffectsProperties = null;
		}

		private void CreateStaticEffects(EffectsProperties effectProperties)
		{
			for (int i = 0; i < effectProperties.Count; i++)
			{
				var effectsPropertiese = effectProperties[i];
				if (effectsPropertiese.IsStatic)
				{
					var group = GetEffectsGroup(effectsPropertiese.EffectName, GamePlace.Game);
					// For case if disabled effect
					if (group != null)
					{
						group.CreateMaxEffects();
					}
				}
			}
		}

		private bool EffectEnabled(string effectName)
		{
			// TODO: QualityManager or something
			return true;
		}

		public void SetEffectsState(EffectType effectType, bool state)
		{
			m_curEffectStates[effectType] = state;
			foreach (var placeGroup in m_effectGroups)
			{
				foreach (var effectsGroup in placeGroup.Value)
				{
					for (int effectIndex = 0; effectIndex < effectsGroup.Value.Effects.Count; effectIndex++)
					{
						var effect = effectsGroup.Value.Effects[effectIndex];
						effect.EffectPlayer.SetState(effectType, state);
					}
				}
			}
		}

		public List<EffectPlayer> StopActiveEffectsByTag(Transform configParent)
		{
			var res = GetActiveEffectsByTag(configParent);
			foreach (var effectPlayer in res)
			{
				effectPlayer.Stop();
			}

			m_activeEffects.RemoveAll(ae => res.Contains(ae));
#if UNITY_EDITOR
			foreach (var effectPlayer in res)
			{
				int index = m_activeEffects.IndexOf(effectPlayer);
				m_activeEffectNames.RemoveAt(index);
			}
#endif
			return res;
		}

		private List<EffectPlayer> GetActiveEffectsByTag(Transform configParent)
		{
			return m_activeEffects.FindAll(ae => ae.transform.parent == configParent);
		}

		// HACK for Unity 5.5.0
		protected void OnApplicationPause(bool isPause) { m_isApplicationPaused = isPause; }
		// End HACK

		private EffectsProperties CurrentEffectsProperties { get; set; }

		public EffectsProperties Properties
		{
			get { return m_effectsProperties; }
		}

		[SerializeField, HideInInspector]
		private EffectsProperties m_effectsProperties = null;

		[SerializeField]
		private string m_path = "Prefabs/Effects/";

		[SerializeField]
		private string m_minigamePath = "Prefabs/Effects/Minigame/";

		private readonly Dictionary<GamePlace, Dictionary<string, EffectsGroup>> m_effectGroups =
			new Dictionary<GamePlace, Dictionary<string, EffectsGroup>>();

		private readonly List<EffectPlayer> m_activeEffects = new List<EffectPlayer>();

		private Dictionary<EffectType, bool> m_curEffectStates = new Dictionary<EffectType, bool>();

		// HACK for Unity 5.5.0
		private bool m_isApplicationPaused = false;
		// End HACK

#if UNITY_EDITOR
		private readonly List<string> m_activeEffectNames = new List<string>();
#endif
	}
}