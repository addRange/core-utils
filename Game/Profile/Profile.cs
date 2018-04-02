//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (2018.01.25)
//----------------------------------------------------------------------------------------------

using System.Collections.Generic;
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
		
		public new static string GetPathToPrefab()
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
				return;
			}
			Data.DeInit();
			Data = JsonUtility.FromJson<TData>(ObscuredPrefs.GetString(MainSaveKey, string.Empty));
			InitData();
		}

		//[ContextMenu("Save")]
		protected void Save()
		{
			ObscuredPrefs.SetString(MainSaveKey, JsonUtility.ToJson(Data));
			ObscuredPrefs.Save();
		}

		//[ContextMenu("Delete All")]
		protected void Clear()
		{
			PlayerPrefs.DeleteAll();
			ObscuredPrefs.DeleteAll();
			Debug.Log("Cleared");
			Data = new TData();
			PlayerPrefs.Save();
		}
		public TData Data { get; private set; }

		/// <summary>
		/// Key that can be changed while not in production for reset saves if change something, that can broke game from old saves)
		/// </summary>
		protected virtual string MainSaveKey
		{
			get { return "ProfileData"; }
		}
	}
}