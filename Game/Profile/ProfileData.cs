//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (2018.01.25)
//----------------------------------------------------------------------------------------------

using System;
using UnityEngine;

namespace Core.Utils.Game
{
	[Serializable]
	public abstract class ProfileData
	{
		public ProfileData()
		{
		}

		/// <summary>
		/// Init profile and check for update version of GameSave
		/// </summary>
		/// <returns>true if new version detected and need save again</returns>
		public virtual bool Init()
		{
			bool isNewVersion = CheckVersions();
			RandomizeCryptoKeys();
			return isNewVersion;
		}

		public virtual void DeInit() {  }

		protected virtual void RandomizeCryptoKeys()
		{
			// for RandomizeCryptoKey from ObscuredTypes
		}
		
		protected virtual bool CheckVersions()
		{
			if (m_version >= Version)
			{
				return false;
			}

			return true;
		}

		// TODO: add BaseSettings with Music and sound)
		// public Settings Settings { get { return m_settings; } }}

		protected virtual int Version { get { return 1; } }

		public int CurVersion { get { return m_version; } }

		[SerializeField]
		protected int m_version = 1;

	}
}