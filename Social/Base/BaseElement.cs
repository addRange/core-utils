//----------------------------------------------------------------------------------------------
// author Leonid [Zanleo] Voitko
//----------------------------------------------------------------------------------------------

using System;
using UnityEngine;

namespace Social
{
	[Serializable]
	public abstract class BaseElement : ScriptableObject
	{
		public virtual void Init()
		{
			Load();
		}

		public virtual void DeInit()
		{
			Save();
		}

		private void Load()
		{
			string res = PlayerPrefs.GetString(GetPrefsId(), string.Empty);
			if (string.IsNullOrEmpty(res))
			{
				return;
			}

			JsonUtility.FromJsonOverwrite(res, this);
		}

		private void Save()
		{
			var res = JsonUtility.ToJson(this);
			PlayerPrefs.SetString(GetPrefsId(), res);
		}

		private string GetPrefsId()
		{
			return GetPrefsPrefix() + "." + m_localId;
		}

		[ContextMenu("Validate")]
		protected void OnValidate()
		{
			Validate(false);
		}

		[ContextMenu("Force AutoConfig")]
		protected virtual void AutoConfig()
		{
			Validate(true);
		}

		protected void Validate(bool force)
		{
			if (force || string.IsNullOrEmpty(m_localId))
			{
				m_localId = name;
			}
		}

		protected abstract string GetPrefsPrefix();

		[SerializeField]
		protected string m_localId = string.Empty;
	}
}