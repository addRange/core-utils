//----------------------------------------------------------------------------------------------
// author Leonid [Zanleo] Voitko
//----------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using Assert = Core.Utils.Assert;

namespace Core.Utils
{
	[XmlRoot("Dictionary")]
	public class XmlSerializableDictionary : Dictionary<string, string>, IXmlSerializable
	{
		public System.Xml.Schema.XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml(System.Xml.XmlReader reader)
		{
			bool wasEmpty = reader.IsEmptyElement;
			if (wasEmpty)
			{
				return;
			}

			Clear();
#if UNITY_EDITOR
			string lastSection = null;
#endif
			while (reader.Read())
			{
				if (reader.Name.Equals(SectionName))
				{
#if UNITY_EDITOR
					if (reader.NodeType == XmlNodeType.Element)
					{
						lastSection = reader.GetAttribute("Name");
						m_editorSections.Add(lastSection, new List<string>());
					}
#endif
					continue;
				}

				if (!reader.MoveToNextAttribute())
				{
					break;
				}

				Assert.IsFalse(this.ContainsKey(reader.Name), reader.Name);
				this.Add(reader.Name, reader.Value);
#if UNITY_EDITOR
				if (lastSection == null)
				{
					lastSection = string.Empty;
				}
				m_editorSections[lastSection].Add(reader.Name);
#endif
			}
		}

		public void WriteXml(XmlWriter writer)
		{
#if UNITY_EDITOR
			foreach (var editorSection in m_editorSections)
			{
				writer.WriteStartElement(SectionName);
				writer.WriteAttributeString("Name", editorSection.Key);
				foreach (var key in editorSection.Value)
				{
					WriteItem(writer, key);
				}
				writer.WriteEndElement(); // End of Section
			}
#else
			foreach (string key in this.Keys)
			{
				WriteItem(writer, key);
			}
#endif
		}
#if UNITY_EDITOR
		public void EditorAddToSection(string section, string key, string value)
		{
			this.Add(key, value);
			if (!m_editorSections.ContainsKey(section))
			{
				m_editorSections.Add(section, new List<string>());
			}
			m_editorSections[section].Add(key);
		}

		public string EditorGetSectionName(string key)
		{
			foreach (var editorSection in m_editorSections)
			{
				if (editorSection.Value.Contains(key))
				{
					return editorSection.Key;
				}
			}
			Debug.LogWarning("Not found section for key=" + key);
			return string.Empty;
		}
#endif

		private void WriteItem(XmlWriter writer, string key)
		{
			writer.WriteStartElement("Item");
			writer.WriteAttributeString(key, this[key]);
			writer.WriteEndElement(); // End of Item
		}

		private const string SectionName = "Section";
#if UNITY_EDITOR
		private Dictionary<string, List<string>> m_editorSections = new Dictionary<string, List<string>>();
#endif
	}
}