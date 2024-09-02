using System.Collections.Generic;
using System.IO;
using System.Xml;
using DocGen.Compression.Zip;
using DocGen.DocIO.DLS.Convertors;

namespace DocGen.DocIO.DLS;

internal class PartContainer
{
	protected string m_name;

	protected Dictionary<string, Part> m_xmlParts;

	protected Dictionary<string, PartContainer> m_xmlPartContainers;

	protected Dictionary<string, Relations> m_relations;

	internal Dictionary<string, Part> XmlParts
	{
		get
		{
			if (m_xmlParts == null)
			{
				m_xmlParts = new Dictionary<string, Part>();
			}
			return m_xmlParts;
		}
	}

	internal Dictionary<string, PartContainer> XmlPartContainers
	{
		get
		{
			if (m_xmlPartContainers == null)
			{
				m_xmlPartContainers = new Dictionary<string, PartContainer>();
			}
			return m_xmlPartContainers;
		}
	}

	internal Dictionary<string, Relations> Relations
	{
		get
		{
			if (m_relations == null)
			{
				m_relations = new Dictionary<string, Relations>();
			}
			return m_relations;
		}
	}

	internal string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			m_name = value;
		}
	}

	internal void AddPart(ZipArchiveItem item)
	{
		Part part = new Part(item.DataStream);
		part.Name = GetPartName(item.ItemName);
		AddPart(part);
	}

	internal void AddPart(Part xmlPart)
	{
		XmlParts.Add(xmlPart.Name, xmlPart);
	}

	internal void AddPartContainer(PartContainer container)
	{
		XmlPartContainers.Add(container.Name, container);
	}

	internal PartContainer EnsurePartContainer(string[] nameParts, int startNameIndex)
	{
		if (nameParts.Length == 1)
		{
			return this;
		}
		string text = nameParts[startNameIndex] + "/";
		PartContainer partContainer = null;
		if (XmlPartContainers.ContainsKey(text))
		{
			partContainer = XmlPartContainers[text];
		}
		if (partContainer == null)
		{
			if (text.EndsWith("_rels/"))
			{
				return this;
			}
			partContainer = new PartContainer();
			partContainer.Name = text;
			AddPartContainer(partContainer);
		}
		if (startNameIndex < nameParts.Length - 2)
		{
			return partContainer.EnsurePartContainer(nameParts, ++startNameIndex);
		}
		return partContainer;
	}

	internal void LoadRelations(ZipArchiveItem item)
	{
		Relations value = new Relations(item);
		Relations.Add(item.ItemName, value);
	}

	private string GetPartName(string fullPath)
	{
		return fullPath[(fullPath.LastIndexOf('/') + 1)..];
	}

	internal PartContainer Clone()
	{
		PartContainer partContainer = new PartContainer();
		partContainer.Name = m_name;
		if (m_xmlParts != null && m_xmlParts.Count > 0)
		{
			foreach (string key in m_xmlParts.Keys)
			{
				partContainer.XmlParts.Add(key, m_xmlParts[key].Clone());
			}
		}
		if (m_relations != null && m_relations.Count > 0)
		{
			foreach (string key2 in m_relations.Keys)
			{
				partContainer.Relations.Add(key2, m_relations[key2].Clone() as Relations);
			}
		}
		if (m_xmlPartContainers != null && m_xmlPartContainers.Count > 0)
		{
			foreach (string key3 in m_xmlPartContainers.Keys)
			{
				partContainer.XmlPartContainers.Add(key3, m_xmlPartContainers[key3].Clone());
			}
		}
		return partContainer;
	}

	internal string CopyXmlPartContainer(PartContainer newContainer, Package srcPackage, string[] parts, int index)
	{
		string result = "";
		for (int i = 0; i < parts.Length; i++)
		{
			if (i >= index)
			{
				if (i != parts.Length - 1)
				{
					string key = parts[i] + "/";
					PartContainer partContainer = new PartContainer();
					result = m_xmlPartContainers[key].CopyXmlPartContainer(partContainer, srcPackage, parts, i + 1);
					newContainer.XmlPartContainers.Add(key, partContainer);
					break;
				}
				result = CopyXmlPartItems(newContainer, srcPackage, parts[i]);
			}
		}
		return result;
	}

	internal string CopyXmlPartItems(PartContainer newContainer, Package srcPackage, string partName)
	{
		string text = "";
		if (newContainer.XmlParts.ContainsKey(partName))
		{
			string text2 = "." + partName.Split('.')[^1];
			string text3 = partName.Replace(text2, "");
			string text4 = text3.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
			int result = 0;
			int.TryParse(text3.Replace(text4, ""), out result);
			text = text4 + result + text2;
			while (newContainer.XmlParts.ContainsKey(text))
			{
				result++;
				text = text4 + result + text2;
			}
		}
		if (m_xmlParts != null && m_xmlParts.ContainsKey(partName))
		{
			Part part = m_xmlParts[partName].Clone();
			if (!string.IsNullOrEmpty(text))
			{
				part.Name = text;
			}
			newContainer.XmlParts.Add(part.Name, part);
		}
		if (m_relations != null)
		{
			string xmlPartRelationKey = GetXmlPartRelationKey(partName);
			if (m_relations.ContainsKey(xmlPartRelationKey))
			{
				Relations relations = m_relations[xmlPartRelationKey].Clone() as Relations;
				if (!string.IsNullOrEmpty(text))
				{
					relations.Name = xmlPartRelationKey.Replace(partName + ".rels", text + ".rels");
				}
				if (newContainer.Relations.ContainsKey(relations.Name))
				{
					newContainer.Relations.Remove(relations.Name);
				}
				newContainer.Relations.Add(relations.Name, relations);
				Dictionary<string, string> innerRelationTarget = CopyInnerRelatedXmlParts(newContainer, srcPackage, relations, partName);
				UpdateInnerRelationTarget(relations, innerRelationTarget);
			}
		}
		return text;
	}

	internal string GetXmlPartRelationKey(string partName)
	{
		string result = "";
		if (m_relations == null)
		{
			return result;
		}
		foreach (string key in m_relations.Keys)
		{
			if (key.EndsWith(partName + ".rels"))
			{
				result = key;
				break;
			}
		}
		return result;
	}

	private void UpdateInnerRelationTarget(Relations relation, Dictionary<string, string> innerRelationTarget)
	{
		Stream dataStream = relation.DataStream;
		dataStream.Position = 0L;
		XmlReader xmlReader = UtilityMethods.CreateReader(dataStream);
		MemoryStream memoryStream = new MemoryStream();
		XmlWriterSettings settings = new XmlWriterSettings();
		XmlWriter xmlWriter = XmlWriter.Create(memoryStream, settings);
		xmlWriter.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"");
		bool flag = false;
		do
		{
			bool flag2 = false;
			switch (xmlReader.NodeType)
			{
			case XmlNodeType.Element:
			{
				xmlWriter.WriteStartElement(xmlReader.Prefix, xmlReader.LocalName, xmlReader.NamespaceURI);
				for (int i = 0; i < xmlReader.AttributeCount; i++)
				{
					xmlReader.MoveToAttribute(i);
					string localName = xmlReader.LocalName;
					if (localName == "target" || localName == "Target")
					{
						xmlWriter.WriteAttributeString(xmlReader.Prefix, xmlReader.LocalName, xmlReader.NamespaceURI, innerRelationTarget.ContainsKey(xmlReader.Value) ? innerRelationTarget[xmlReader.Value] : xmlReader.Value);
					}
					else
					{
						xmlWriter.WriteAttributeString(xmlReader.Prefix, xmlReader.LocalName, xmlReader.NamespaceURI, xmlReader.Value);
					}
				}
				xmlReader.MoveToElement();
				if (!xmlReader.IsEmptyElement)
				{
					string localName2 = xmlReader.LocalName;
					xmlReader.Read();
					flag2 = true;
					if (localName2 == xmlReader.LocalName && xmlReader.NodeType == XmlNodeType.EndElement)
					{
						xmlWriter.WriteEndElement();
					}
				}
				else
				{
					xmlWriter.WriteEndElement();
				}
				break;
			}
			case XmlNodeType.Text:
				xmlWriter.WriteString(xmlReader.Value);
				break;
			case XmlNodeType.EndElement:
				xmlWriter.WriteEndElement();
				break;
			case XmlNodeType.SignificantWhitespace:
				xmlWriter.WriteWhitespace(xmlReader.Value);
				break;
			}
			flag = xmlReader.EOF;
			if (!flag2 && !flag)
			{
				xmlReader.Read();
			}
		}
		while (!flag);
		xmlWriter.Flush();
		memoryStream.Flush();
		memoryStream.Position = 0L;
		relation.SetDataStream(memoryStream);
	}

	private Dictionary<string, string> CopyInnerRelatedXmlParts(PartContainer newContainer, Package srcPackage, Relations relation, string curPartName)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		Stream dataStream = relation.DataStream;
		dataStream.Position = 0L;
		XmlReader xmlReader = UtilityMethods.CreateReader(dataStream);
		xmlReader.MoveToContent();
		if (xmlReader.LocalName != "Relationships")
		{
			xmlReader.ReadInnerXml();
			return dictionary;
		}
		if (!xmlReader.IsEmptyElement)
		{
			string localName = xmlReader.LocalName;
			xmlReader.Read();
			bool flag = true;
			if (localName == xmlReader.LocalName && xmlReader.NodeType == XmlNodeType.EndElement)
			{
				return dictionary;
			}
			do
			{
				if (!flag)
				{
					xmlReader.Read();
				}
				if (!(xmlReader.GetAttribute("TargetMode") == "External"))
				{
					string attribute = xmlReader.GetAttribute("Target");
					if (!string.IsNullOrEmpty(attribute))
					{
						if (attribute.StartsWith("../"))
						{
							PartContainer xmlPartContainer = srcPackage.GetXmlPartContainer(this, attribute);
							string text = attribute.Substring(attribute.LastIndexOf('/') + 1);
							PartContainer partContainer = null;
							if (newContainer.XmlPartContainers.ContainsKey("embeddings/"))
							{
								partContainer = newContainer.XmlPartContainers["embeddings/"];
							}
							else
							{
								partContainer = new PartContainer
								{
									Name = "embeddings/"
								};
								newContainer.XmlPartContainers.Add(partContainer.Name, partContainer);
							}
							string text2 = xmlPartContainer.CopyXmlPartItems(partContainer, srcPackage, text);
							if (!string.IsNullOrEmpty(text2))
							{
								dictionary.Add(attribute, partContainer.Name + text2);
							}
							else
							{
								dictionary.Add(attribute, partContainer.Name + text);
							}
						}
						else
						{
							string[] array = attribute.Split('/');
							string text3 = CopyXmlPartContainer(newContainer, srcPackage, array, 0);
							if (!string.IsNullOrEmpty(text3))
							{
								dictionary.Add(attribute, attribute.Replace(array[^1], text3));
							}
						}
					}
				}
				flag = false;
			}
			while (xmlReader.LocalName != "Relationships");
		}
		return dictionary;
	}

	internal PartContainer GetXmlPartContainer(PartContainer srcContainer, string target)
	{
		string[] nameParts = GetXmlPartContainerPath(srcContainer, target).Split('/');
		return EnsurePartContainer(nameParts, 0);
	}

	internal string GetXmlPartContainerPath(PartContainer container, string target)
	{
		string[] array = target.Split('/');
		int num = 0;
		for (int i = 0; i < array.Length && array[i].Trim('.') == ""; i++)
		{
			num++;
		}
		string text = GetParentPartPath(container);
		while (num > 1)
		{
			text = text.TrimEnd('/');
			text = ((!text.Contains("/")) ? "" : text.Remove(text.LastIndexOf('/')));
			num--;
		}
		return text + target.TrimStart('.', '/');
	}

	private string GetParentPartPath(PartContainer srcContainer)
	{
		string text = "";
		if (m_xmlPartContainers == null)
		{
			return text;
		}
		if (m_xmlPartContainers.ContainsValue(srcContainer))
		{
			return text + Name;
		}
		foreach (KeyValuePair<string, PartContainer> xmlPartContainer in m_xmlPartContainers)
		{
			if (xmlPartContainer.Value.m_xmlPartContainers != null && xmlPartContainer.Value.m_xmlPartContainers.Count != 0)
			{
				string parentPartPath = xmlPartContainer.Value.GetParentPartPath(srcContainer);
				if (!string.IsNullOrEmpty(parentPartPath))
				{
					text = text + Name + parentPartPath;
					break;
				}
			}
		}
		return text;
	}

	internal virtual void Close()
	{
		if (m_xmlParts != null)
		{
			foreach (Part value in m_xmlParts.Values)
			{
				value.Close();
			}
			m_xmlParts.Clear();
			m_xmlParts = null;
		}
		if (m_xmlPartContainers != null)
		{
			foreach (PartContainer value2 in m_xmlPartContainers.Values)
			{
				value2.Close();
			}
			m_xmlPartContainers.Clear();
			m_xmlPartContainers = null;
		}
		if (m_relations == null)
		{
			return;
		}
		foreach (Relations value3 in m_relations.Values)
		{
			value3.Close();
		}
		m_relations.Clear();
		m_relations = null;
	}
}
