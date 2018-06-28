//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (2017.02.09)
//----------------------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;

namespace Localization.Utils
{
	public class TextTranslate : MonoBehaviour
	{
		private void Awake()
		{
			RefreshView();
			TextManager.Instance.EventLanguageChanged += OnLanguageChanged;
		}

		private void OnDestroy()
		{
			if (TextManager.TryInstance)
			{
				TextManager.Instance.EventLanguageChanged -= OnLanguageChanged;
			}
		}

		private void RefreshView()
		{
			var text = GetComponent<Text>();
			text.text = m_id.Translate();
		}

		private void OnLanguageChanged() { RefreshView(); }

		public void SetId(string id)
		{
			m_id = id;
			RefreshView();
		}

		[SerializeField]
		private string m_id = string.Empty;
	}
}