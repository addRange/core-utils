//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (2018.01.25)
//----------------------------------------------------------------------------------------------

//#define Debug_Profile

using System.Collections.Generic;
using System.Diagnostics;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;
using UnityEngine.Analytics;
using Debug = UnityEngine.Debug;

namespace Core.Utils.Game
{
	public abstract class Profile<T, TData> : SingletonGameObject<T>
		where T : Profile<T, TData>
		where TData : ProfileData, new()
	{
		[Conditional("Debug_Profile"), DebuggerStepThrough]
		private void Log(string msg)
		{
			Debug.Log(this + "(" + this.GetHashCode() + "). " + msg, this);
		}
		
		protected override void Init()
		{
			base.Init();
			Data = new TData();
			InitData();
			ObscuredPrefs.lockToDevice = ObscuredPrefs.DeviceLockLevel.Strict;
			ObscuredPrefs.readForeignSaves = false;
			ObscuredPrefs.onAlterationDetected += OnAlterationDetected;
			Load();
		}

		protected override void DeInit()
		{
			Save();
			ObscuredPrefs.onAlterationDetected -= OnAlterationDetected;
			base.DeInit();
		}

		private void InitData()
		{
			if (Data.Init())
			{
				Save();
			}
		}

		private void OnAlterationDetected()
		{
			Analytics.CustomEvent("OnAlterationDetected", new Dictionary<string, object>
			{
				{"ProfileDataVersion", Data.CurVersion},
				{"DeviceId", ObscuredPrefs.DeviceId},
				{"CryptoKey", ObscuredPrefs.CryptoKey},
				{"LockToDevice", ObscuredPrefs.lockToDevice},
			});
		}

		private void OnApplicationFocus(bool isFocus)
		{
			if (!isFocus)
			{
				Save();
			}
		}
		
		public new static string GetPathToAsset()
		{
			var pathToRes = PathToPrefabs + "Profile";
			return pathToRes;
		}

		// ContextMenu  cannot be used from generic classes. so - need write context on inherited
		//[ContextMenu("Load")]
		protected void Load()
		{
			string res = ObscuredPrefs.GetString(MainSaveKey, string.Empty);
			if (string.IsNullOrEmpty(res))
			{
				Log("Load No data " + ";" + MainSaveKey);
				return;
			}

			if (Data != null)
			{
				Log("Deinit old data " + JsonUtility.ToJson(Data, true));
				Data.DeInit();
			}
			Data = JsonUtility.FromJson<TData>(ObscuredPrefs.GetString(MainSaveKey, string.Empty));
			Log("Load data " + JsonUtility.ToJson(Data, true) + ";" + MainSaveKey);
			InitData();
		}

		//[ContextMenu("Save")]
		protected void Save()
		{
			Log("Save '" + MainSaveKey + "'; " + GetObjString(Data) + "; " + JsonUtility.ToJson(Data, true));
			ObscuredPrefs.SetString(MainSaveKey, JsonUtility.ToJson(Data));
			ObscuredPrefs.Save();
		}

		//[ContextMenu("Delete All")]
		protected void Clear()
		{
			ObscuredPrefs.DeleteAll();
			Log("Cleared");
			Data = new TData();
			ObscuredPrefs.Save();
		}

		protected virtual string GetObjString(ProfileData obj) { return (obj == null ? "null" : obj.GetHashCode().ToString()); }

		public TData Data
		{
			get { return m_data; }
			private set
			{
				Log(GetObjString(m_data) + " => " + GetObjString(value));
				m_data = value;
			}
		}

		/// <summary>
		/// Key that can be changed while not in production for reset saves if change something, that can broke game from old saves)
		/// </summary>
		protected virtual string MainSaveKey
		{
			get { return "ProfileData"; }
		}

		private TData m_data;
	}
}