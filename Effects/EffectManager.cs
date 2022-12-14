//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (1/10/2016)
//----------------------------------------------------------------------------------------------

// later need write editor for autoConfigurates statics and max play count
// mode of collecting EffectInfo statistic
//#define COLLECT_STATS_MODE
#if UNITY_EDITOR
#define DETECT_REMOVED_EFFECTS_NAMES
#endif

using System;
using System.Collections.Generic;
using UnityEngine;
using Assert = Core.Utils.Assert;
using UnityEngine.Profiling;
#if COLLECT_STATS_MODE
using System.Text;
#endif
#if DETECT_REMOVED_EFFECTS_NAMES
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
				string fullPath = path + effectsProperties.EffectPath;
				//Debug.Log(effectsProperties.EffectName + "; " + fullPath);
				Resource = Resources.Load<GameObject>(fullPath);
				if (Resource == null)
				{
					effectsProperties.MaxCount = 0;
					Assert.IsTrue(false, "Can't load effect at '" + fullPath + "'");
				}
				else
				{
					// Or not disable, because on device we not load resources
//#if !UNITY_EDITOR
//					// disable resource for not enable/disable instantiated
//					Resource.gameObject.SetActive(false);
//#endif
				}
			}
			
			private EffectInfo GetEffectInfoForPlay(int priority)
			{
				EffectInfo best = null;
				if (Effects.Count > 0)
				{
					++m_lastUsedEffectIndex;
					m_lastUsedEffectIndex %= Effects.Count;
					for (int i = 0; i < Effects.Count; ++i)
					{
						int targIndex = m_lastUsedEffectIndex + i;
						targIndex %= Effects.Count;
						var targEffectInfo = Effects[targIndex];
						if (m_activeEffects.Contains(targEffectInfo.EffectPlayer))
						{
							continue;
						}
						best = targEffectInfo;
						m_lastUsedEffectIndex = targIndex;
					}
				}
				//EffectInfo best = Effects.Find(ei => !activeEffects.Contains(ei.EffectPlayer));
				if (best == null)
				{
					if (Effects.Count < EffectsProperties.MaxCount)
					{
						best = CreateEffectInfo();
						m_lastUsedEffectIndex = Effects.Count - 1;
					}
					else
					{
						var possibleEffects = Effects;//.FindAll(e => e.Priority <= priority);
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
											m_lastUsedEffectIndex = i;
										}
									}
									break;
								case PriorityType.Priority:
									for (int i = 1; i < possibleEffects.Count; i++)
									{
										if (best.Priority >= possibleEffects[i].Priority)
										{
											best = possibleEffects[i];
											m_lastUsedEffectIndex = i;
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

			public void CreateStaticEffects()
			{
				if (!EffectsProperties.IsStatic)
				{
					return;
				}
				int staticCount = EffectsProperties.StaticCount;
				if (staticCount == 0)
				{
					staticCount = EffectsProperties.MaxCount;
				}
				for (int i = Effects.Count; i < staticCount; ++i)
				{
					CreateEffectInfo();
				}
			}

			public void Unload()
			{
				Profiler.BeginSample("EffectGroup.Unload");
				for (int j = 0; j < m_activeEffects.Count; j++)
				{
					var effectPlayer = m_activeEffects[j];
					effectPlayer.Stop();
				}

#if DETECT_REMOVED_EFFECTS_NAMES
				m_activeEffectNames.Clear();
#endif
				m_activeEffects.Clear();
				for (int i = 0; i < Effects.Count; i++)
				{
					var effectInfo = Effects[i];
					DestroyImmediate(effectInfo.EffectPlayer.gameObject);
				}
				Effects.Clear();
				Resources.UnloadAsset(Resource);
				Resource = null;

				Profiler.EndSample();
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
			private bool Play(EffectInfo effectInfo, Transform parent, Vector3 pos, Quaternion rot, out EffectPlayer effectPlayer)
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

			public EffectPlayer TryPlayEffect(
				int priority, Transform parent, Vector3 pos, Quaternion rot, Dictionary<EffectType, bool> curEffectStates)
			{
				EffectInfo effectInfo = GetEffectInfoForPlay(priority);
				if (effectInfo == null)
				{
					Profiler.EndSample();
					return null;
				}

				foreach (var curEffectState in curEffectStates)
				{
					effectInfo.EffectPlayer.SetState(curEffectState.Key, curEffectState.Value);
				}
				EffectPlayer player = null;
				bool isTerminated = Play(effectInfo, parent, pos, rot, out player);
				if (isTerminated)
				{
#if DETECT_REMOVED_EFFECTS_NAMES
				int index = m_activeEffects.IndexOf(player);
				m_activeEffectNames.RemoveAt(index);
#endif
					m_activeEffects.Remove(player);
				}
				m_activeEffects.Add(player);
#if DETECT_REMOVED_EFFECTS_NAMES
			m_activeEffectNames.Add(player.name);
#endif
				return player;
			}

			public void UnloadEffects()
			{
				for (int i = 0; i < m_activeEffects.Count; i++)
				{
					var mActiveEffect = m_activeEffects[i];
#if DETECT_REMOVED_EFFECTS_NAMES
				Assert.IsNotNull(mActiveEffect, m_activeEffectNames[i]);
#endif
					mActiveEffect.Stop();
				}
				m_activeEffects.Clear();
#if DETECT_REMOVED_EFFECTS_NAMES
			m_activeEffectNames.Clear();
#endif
			}

			public void Tick()
			{
				for (var i = 0; i < m_activeEffects.Count; i++)
				{
					var activeEffectPlayer = m_activeEffects[i];
#if DETECT_REMOVED_EFFECTS_NAMES
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
#if DETECT_REMOVED_EFFECTS_NAMES
					m_activeEffectNames.RemoveAt(i);
#endif
					--i;
					Profiler.EndSample();
				}
			}

			public void CollectAllChildActiveEffects()
			{
				List<EffectPlayer> notDisabledEffects = new List<EffectPlayer>();
				for (int i = 0; i < m_activeEffects.Count; i++)
				{
					var e = m_activeEffects[i];
#if DETECT_REMOVED_EFFECTS_NAMES
					Assert.IsNotNull(e, m_activeEffectNames[i]);
#endif
					if (m_activeEffects[i].transform.parent == null)
					{
						notDisabledEffects.Add(m_activeEffects[i]);
						continue;
					}
					e.Stop();
					e.transform.SetParent(m_effectManager.transform, false);
				}
				m_activeEffects.Clear();
				m_activeEffects.AddRange(notDisabledEffects);

#if DETECT_REMOVED_EFFECTS_NAMES
				m_activeEffectNames.Clear();
				m_activeEffectNames.AddRange(m_activeEffects.Select(e => e.name));
#endif
			}

			public List<EffectPlayer> StopActiveEffectsByTag(Transform configParent)
			{
				var res = GetActiveEffectsByTag(configParent);
				foreach (var effectPlayer in res)
				{
					effectPlayer.Stop();
				}
				m_activeEffects.RemoveAll(ae => res.Contains(ae));
#if DETECT_REMOVED_EFFECTS_NAMES
				foreach (var effectPlayer in res)
				{
					int index = m_activeEffects.IndexOf(effectPlayer);
					m_activeEffectNames.RemoveAt(index);
				}
#endif
				return res;
			}

			public List<EffectPlayer> GetActiveEffectsByTag(Transform configParent)
			{
				return m_activeEffects.FindAll(ae => ae.transform.parent == configParent);
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

			private readonly List<EffectPlayer> m_activeEffects = new List<EffectPlayer>();
			private int m_lastUsedEffectIndex = -1;
#if DETECT_REMOVED_EFFECTS_NAMES
			private readonly List<string> m_activeEffectNames = new List<string>();
#endif
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

		private void OnApplicationQuit() { UnloadEffects(); }

		private void UnloadEffects()
		{
#if COLLECT_STATS_MODE
		StringBuilder sb = new StringBuilder();
#endif
			foreach (var effectGroupsValue in m_effectGroups.Values)
			{
				foreach (var effectsGroup in effectGroupsValue.Values)
				{
#if COLLECT_STATS_MODE
				effectsGroup.GetPrintCollectedStats(sb);
				sb.AppendLine();
#endif
					effectsGroup.UnloadEffects();
				}
			}
#if COLLECT_STATS_MODE
		if (sb.Length > 0)
		{
			Debug.Log(sb.ToString());
		}
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
			for (int i = 0; i < m_effectGroupsOnly.Count; ++i)
			{
				var effectGroup = m_effectGroupsOnly[i];
				effectGroup.Tick();
			}
			Profiler.EndSample();
		}

		public void CollectAllChildActiveEffects()
		{
			foreach (var effectsGroup in m_effectGroupsOnly)
			{
				effectsGroup.CollectAllChildActiveEffects();
			}
		}

		/// <summary>
		/// Create effect and start playing in parent
		/// </summary>
		/// <param name="id"></param>
		/// <param name="gamePlace"></param>
		/// <param name="parent"></param>
		/// <returns>Created effectPlayer or null if can`t play</returns>
		public EffectPlayer PlayEffect(string name, GamePlace gamePlace = GamePlace.Game, Transform parent = null, int priority = 0)
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
		public EffectPlayer PlayEffect(string name, GamePlace gamePlace, Transform parent, Vector3 pos, Quaternion rot, int priority)
		{
			Profiler.BeginSample("EffectManager.PlayEffect()", gameObject);
			var group = GetEffectsGroup(name, gamePlace);
			// FOr disabled effects
			if (group == null)
			{
				Profiler.EndSample();
				return null;
			}

			EffectPlayer player = null;
			player = group.TryPlayEffect(priority, parent, pos, rot, m_curEffectStates);
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
				m_effectGroupsOnly.Add(newGroup);
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
				effectsGroup.Value.Unload();
				m_effectGroupsOnly.Remove(effectsGroup.Value);
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
						group.CreateStaticEffects();
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
			List<EffectPlayer> res = new List<EffectPlayer>();
			foreach (var effectGroupsValue in m_effectGroups.Values)
			{
				foreach (var effectsGroup in effectGroupsValue.Values)
				{
					var subRes = effectsGroup.StopActiveEffectsByTag(configParent);
					res.AddRange(subRes);
				}
			}
			return res;
		}

		private List<EffectPlayer> GetActiveEffectsByTag(Transform configParent)
		{
			List<EffectPlayer> res = new List<EffectPlayer>();
			foreach (var effectGroupsValue in m_effectGroups.Values)
			{
				foreach (var effectsGroup in effectGroupsValue.Values)
				{
					var subRes = effectsGroup.GetActiveEffectsByTag(configParent);
					res.AddRange(subRes);
				}
			}

			return res;
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
		private List<EffectsGroup> m_effectGroupsOnly = new List<EffectsGroup>();
		private Dictionary<EffectType, bool> m_curEffectStates = new Dictionary<EffectType, bool>();

		// HACK for Unity 5.5.0
		private bool m_isApplicationPaused = false;
		// End HACK
	}
}