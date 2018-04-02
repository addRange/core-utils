//----------------------------------------------------------------------------------------------
// author Leonid [Zanleo] Voitko
//----------------------------------------------------------------------------------------------

using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using Core.Utils;
using Debug = UnityEngine.Debug;

namespace Social
{
	public abstract class BaseIds<T, T2> : SingletonGameObject<T> where T : BaseIds<T, T2> where T2 : BaseElement
	{
		protected override void Init()
		{
			base.Init();
			LoadIds(Social.Instance.GetSocialPlatform());
		}

		private void LoadIds(SocialPlatform currentplatform)
		{
			string path = string.Format(m_path, typeof(T2).Name, currentplatform.ToString());
			var textAsset = Resources.Load<TextAsset>(path);
			if (textAsset == null)
			{
				Debug.LogError("Wrong loaded platform by path " + path);
				return;
			}

			if (m_dictionary.Count > 0)
			{
				m_dictionary.Clear();
			}
			var serializer = new XmlSerializer(typeof(XmlSerializableDictionary));
			using (var reader = new StringReader(textAsset.text))
			{
				m_dictionary = serializer.Deserialize(reader) as XmlSerializableDictionary;
			}
			Resources.UnloadAsset(textAsset);
		}

		/// <summary>
		/// Get id for local social element id
		/// </summary>
		/// <param name="localId"></param>
		/// <returns></returns>
		public string GetId(string localId)
		{
			if (!m_dictionary.ContainsKey(localId))
			{
				return localId;
			}

			return m_dictionary[localId];
		}

		[SerializeField, Tooltip("params 0 - type of social element; 1 - platform")]
		private string m_path = @"Configs/Social/{0}/{1}";

		private XmlSerializableDictionary m_dictionary = new XmlSerializableDictionary();
	}
}