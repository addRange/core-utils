//----------------------------------------------------------------------------------------------
// author Leonid [Zanleo] Voitko
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

public class TextManagerReplaceLanguages : ScriptableObject
{
	[Serializable]
	private class LangForReplace
	{
		public SystemLanguage LanguageForReplace
		{
			get { return m_languageForReplace; }
		}

		public SystemLanguage LangWhichReplace
		{
			get { return m_langWhichReplace; }
		}

		[SerializeField]
		private SystemLanguage m_languageForReplace = SystemLanguage.English;

		[SerializeField]
		private SystemLanguage m_langWhichReplace = SystemLanguage.English;
	}

	public SystemLanguage TryGetCorrectedLanguage(SystemLanguage forCorrection)
	{
		var targ = m_replaceList.Find(r => r.LanguageForReplace == forCorrection);
		if (targ == null)
		{
			return SystemLanguage.Unknown;
		}

		return targ.LangWhichReplace;
	}

	[SerializeField]
	private List<LangForReplace> m_replaceList = new List<LangForReplace>();
}