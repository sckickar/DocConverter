using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using DocGen.DocIO.DLS.Convertors;

namespace DocGen.DocIO.DLS;

public class XmlMapping
{
	private string m_prefixMapping;

	private string m_XPath;

	private string m_storeItemID;

	private byte m_bFlags;

	private CustomXMLPart m_customXMLPart;

	private CustomXMLNode m_customXMLNode;

	private Entity m_ownerControl;

	public bool IsMapped
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		internal set
		{
			m_bFlags = (byte)((m_bFlags & 0xFEu) | (value ? 1u : 0u));
		}
	}

	public string PrefixMapping
	{
		get
		{
			return m_prefixMapping;
		}
		internal set
		{
			m_prefixMapping = value;
		}
	}

	public string XPath
	{
		get
		{
			return m_XPath;
		}
		internal set
		{
			m_XPath = value;
		}
	}

	internal string StoreItemID
	{
		get
		{
			return m_storeItemID;
		}
		set
		{
			m_storeItemID = value;
		}
	}

	public CustomXMLPart CustomXmlPart
	{
		get
		{
			if (m_customXMLPart == null)
			{
				m_customXMLPart = new CustomXMLPart();
			}
			return m_customXMLPart;
		}
	}

	public CustomXMLNode CustomXmlNode
	{
		get
		{
			if (m_customXMLNode == null)
			{
				m_customXMLNode = new CustomXMLNode();
			}
			return m_customXMLNode;
		}
	}

	internal bool IsWordML
	{
		get
		{
			return (m_bFlags & 4) >> 2 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	internal bool IsSupportWordML
	{
		get
		{
			return (m_bFlags & 8) >> 3 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xF7u) | ((value ? 1u : 0u) << 3));
		}
	}

	internal XmlMapping(Entity ownerControl)
	{
		m_ownerControl = ownerControl;
	}

	public void SetMapping(string xPath, string prefixMapping, CustomXMLPart customXmlPart)
	{
		XPath = xPath;
		PrefixMapping = prefixMapping;
		StoreItemID = customXmlPart.Id;
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(customXmlPart.XML);
		XmlNamespaceManager xmlNamespaceManager = GetXmlNamespaceManager(xmlDocument, PrefixMapping);
		try
		{
			XmlNode xmlNode = xmlDocument.SelectSingleNode(xPath, xmlNamespaceManager);
			if (xmlNode != null)
			{
				MapItemsToControl(xmlNode.InnerText);
			}
		}
		catch (XPathException)
		{
		}
	}

	public void Delete()
	{
		XPath = null;
		PrefixMapping = null;
		StoreItemID = null;
	}

	public void SetMappingByNode(CustomXMLNode customXmlNode)
	{
		XPath = customXmlNode.XPath;
		StoreItemID = customXmlNode.OwnerPart.Id;
		System.Xml.Linq.XNode xNode = null;
		xNode = System.Xml.Linq.XDocument.Parse(customXmlNode.OwnerPart.XML).SelectSingleNode(XPath);
		if (xNode != null)
		{
			MapItemsToControl(((XElement)xNode).Value);
		}
	}

	private void MapItemsToControl(string text)
	{
		if (m_ownerControl != null && m_ownerControl is InlineContentControl)
		{
			WTextRange wTextRange = new WTextRange((m_ownerControl as InlineContentControl).Document);
			wTextRange.Text = text;
			(m_ownerControl as InlineContentControl).ParagraphItems.Add(wTextRange);
		}
		else if (m_ownerControl != null && m_ownerControl is BlockContentControl)
		{
			WParagraph wParagraph = new WParagraph((m_ownerControl as BlockContentControl).Document);
			wParagraph.AppendText(text);
			(m_ownerControl as BlockContentControl).MappedParagraph = wParagraph;
			wParagraph.SetOwner(m_ownerControl as BlockContentControl);
			(m_ownerControl as BlockContentControl).ContentControlProperties.XmlMapping.IsMapped = true;
		}
		else if (m_ownerControl != null && m_ownerControl is WTableCell)
		{
			WTableCell wTableCell = m_ownerControl as WTableCell;
			WTableCell wTableCell2 = new WTableCell(wTableCell.Document);
			wTableCell2.AddParagraph().AppendText(text);
			wTableCell2.SetOwner(wTableCell.OwnerRow);
			wTableCell2.OwnerRow.SetOwner(wTableCell.OwnerRow.OwnerTable);
			wTableCell2.ContentControl = wTableCell.ContentControl;
			wTableCell2.ContentControl.ContentControlProperties.XmlMapping.IsMapped = true;
		}
	}

	internal string GetOrUpdateMappedValue(WordDocument document, InlineContentControl control, XmlElement rootElement, string modifiedText, bool isMappedwithCustomXML, ref bool isRootElement, ref bool isDocProperty, ref bool isMappedElementHasNoValue)
	{
		string result = string.Empty;
		if (!string.IsNullOrEmpty(control.ContentControlProperties.XmlMapping.XPath))
		{
			List<string> listPath = GetListPath(control.ContentControlProperties.XmlMapping.XPath, ref isDocProperty);
			if (isDocProperty && !isMappedwithCustomXML)
			{
				result = SetCoreProperty(control, listPath, document, modifiedText);
			}
			else if (rootElement != null)
			{
				if (listPath.Count == 1)
				{
					isRootElement = true;
				}
				XmlDocument ownerDocument = rootElement.OwnerDocument;
				XmlNamespaceManager xmlNamespaceManager = GetXmlNamespaceManager(ownerDocument, control.ContentControlProperties.XmlMapping.PrefixMapping);
				try
				{
					XmlNode xmlNode = ownerDocument.SelectSingleNode(control.ContentControlProperties.XmlMapping.XPath, xmlNamespaceManager);
					if (xmlNode != null && xmlNode.NodeType == XmlNodeType.Element)
					{
						if (!string.IsNullOrEmpty(modifiedText))
						{
							xmlNode.InnerText = modifiedText;
							return string.Empty;
						}
						result = xmlNode.InnerText;
					}
					else
					{
						isMappedElementHasNoValue = true;
					}
				}
				catch
				{
					isMappedElementHasNoValue = true;
				}
			}
			else
			{
				isMappedElementHasNoValue = true;
			}
		}
		return result;
	}

	internal string GetOrUpdateMappedValue(WordDocument document, BlockContentControl blockControl, XmlElement rootElement, string modifiedText, ref bool isRootElement, ref bool isDocProperty, ref bool isMappedElementHasNoValue)
	{
		string result = string.Empty;
		if (!string.IsNullOrEmpty(blockControl.ContentControlProperties.XmlMapping.XPath))
		{
			List<string> listPath = GetListPath(blockControl.ContentControlProperties.XmlMapping.XPath, ref isDocProperty);
			if (isDocProperty)
			{
				result = SetCoreProperty(blockControl, listPath, document, modifiedText);
			}
			else if (rootElement != null)
			{
				if (listPath.Count == 1)
				{
					isRootElement = true;
				}
				XmlDocument ownerDocument = rootElement.OwnerDocument;
				XmlNamespaceManager xmlNamespaceManager = GetXmlNamespaceManager(ownerDocument, blockControl.ContentControlProperties.XmlMapping.PrefixMapping);
				try
				{
					XmlNode xmlNode = ownerDocument.SelectSingleNode(blockControl.ContentControlProperties.XmlMapping.XPath, xmlNamespaceManager);
					if (xmlNode != null && xmlNode.NodeType == XmlNodeType.Element)
					{
						if (!string.IsNullOrEmpty(modifiedText))
						{
							xmlNode.InnerText = modifiedText;
							return string.Empty;
						}
						result = xmlNode.InnerText;
					}
					else
					{
						isMappedElementHasNoValue = true;
					}
				}
				catch
				{
					isMappedElementHasNoValue = true;
				}
			}
			else
			{
				isMappedElementHasNoValue = true;
			}
		}
		return result;
	}

	internal XmlNamespaceManager GetXmlNamespaceManager(XmlDocument xmlDocument, string prefixMapping)
	{
		XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
		if (!string.IsNullOrEmpty(prefixMapping))
		{
			string pattern = "xmlns:(\\w+)='([^']*)'";
			foreach (Match item in Regex.Matches(prefixMapping, pattern))
			{
				string value = item.Groups[1].Value;
				string value2 = item.Groups[2].Value;
				xmlNamespaceManager.AddNamespace(value, value2);
			}
		}
		else
		{
			XmlElement? documentElement = xmlDocument.DocumentElement;
			int namespaceIndex = 0;
			ProcessNode(documentElement, xmlNamespaceManager, ref namespaceIndex);
		}
		return xmlNamespaceManager;
	}

	private static void ProcessNode(XmlNode node, XmlNamespaceManager namespaceManager, ref int namespaceIndex)
	{
		if (node.NodeType == XmlNodeType.Element)
		{
			foreach (XmlAttribute attribute in node.Attributes)
			{
				if (attribute.Name.Contains("xmlns"))
				{
					string prefix = "ns" + namespaceIndex;
					namespaceIndex++;
					string value = attribute.Value;
					if (!IsNamespaceURIExists(namespaceManager, value))
					{
						namespaceManager.AddNamespace(prefix, value);
					}
				}
			}
		}
		foreach (XmlNode childNode in node.ChildNodes)
		{
			ProcessNode(childNode, namespaceManager, ref namespaceIndex);
		}
	}

	private static bool IsNamespaceURIExists(XmlNamespaceManager namespaceManager, string uri)
	{
		foreach (string item in namespaceManager)
		{
			if (namespaceManager.LookupNamespace(item) == uri)
			{
				return true;
			}
		}
		return false;
	}

	private List<string> GetListPath(string path, ref bool isDocProperty)
	{
		string[] array = path.Split(new char[1] { '/' });
		List<string> list = new List<string>();
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (text != "")
			{
				list.Add(text);
			}
			if (text.ToLower().Contains("coreproperties") || text.ToLower().Contains("coverpageproperties"))
			{
				isDocProperty = true;
			}
		}
		return list;
	}

	private string SetCoreProperty(object control, List<string> xPathList, WordDocument m_doc, string modifiedText)
	{
		string result = string.Empty;
		foreach (string xPath in xPathList)
		{
			if (xPath.Contains("coreProperties"))
			{
				continue;
			}
			int startIndex = 0;
			int endIndex = 0;
			string text = GetPathIndex(xPath, ref startIndex, ref endIndex);
			if ((startIndex == -1 || endIndex == -1) && (startIndex != -1 || endIndex != -1))
			{
				continue;
			}
			if (startIndex != -1 && endIndex != -1)
			{
				text = text.Substring(0, startIndex);
			}
			if (string.IsNullOrEmpty(modifiedText))
			{
				switch (text)
				{
				case "title":
					result = m_doc.BuiltinDocumentProperties.Title;
					break;
				case "creator":
					result = m_doc.BuiltinDocumentProperties.Author;
					break;
				case "category":
					result = m_doc.BuiltinDocumentProperties.Category;
					break;
				case "created":
					result = Convert.ToString(m_doc.BuiltinDocumentProperties.CreateDate, CultureInfo.InvariantCulture);
					break;
				case "description":
					result = m_doc.BuiltinDocumentProperties.Comments;
					break;
				case "keywords":
					result = m_doc.BuiltinDocumentProperties.Keywords;
					break;
				case "lastModifiedBy":
					result = m_doc.BuiltinDocumentProperties.LastAuthor;
					break;
				case "lastPrinted":
					result = Convert.ToString(m_doc.BuiltinDocumentProperties.LastPrinted, CultureInfo.InvariantCulture);
					break;
				case "modified":
					result = Convert.ToString(m_doc.BuiltinDocumentProperties.LastSaveDate, CultureInfo.InvariantCulture);
					break;
				case "subject":
					result = m_doc.BuiltinDocumentProperties.Subject;
					break;
				case "revision":
					result = m_doc.BuiltinDocumentProperties.RevisionNumber;
					break;
				case "contentStatus":
					result = m_doc.BuiltinDocumentProperties.ContentStatus;
					break;
				}
			}
			else
			{
				switch (text)
				{
				case "title":
					m_doc.BuiltinDocumentProperties.Title = modifiedText;
					result = m_doc.BuiltinDocumentProperties.Title;
					break;
				case "creator":
					m_doc.BuiltinDocumentProperties.Author = modifiedText;
					result = m_doc.BuiltinDocumentProperties.Author;
					break;
				case "category":
					m_doc.BuiltinDocumentProperties.Category = modifiedText;
					result = m_doc.BuiltinDocumentProperties.Category;
					break;
				case "description":
					m_doc.BuiltinDocumentProperties.Comments = modifiedText;
					result = m_doc.BuiltinDocumentProperties.Comments;
					break;
				case "keywords":
					m_doc.BuiltinDocumentProperties.Keywords = modifiedText;
					result = m_doc.BuiltinDocumentProperties.Keywords;
					break;
				case "subject":
					m_doc.BuiltinDocumentProperties.Subject = modifiedText;
					result = m_doc.BuiltinDocumentProperties.Subject;
					break;
				case "contentStatus":
					m_doc.BuiltinDocumentProperties.ContentStatus = modifiedText;
					result = m_doc.BuiltinDocumentProperties.ContentStatus;
					break;
				}
			}
		}
		return result;
	}

	private string GetPathIndex(string path, ref int startIndex, ref int endIndex)
	{
		if (path.Contains(":"))
		{
			path = path.Substring(path.IndexOf(":") + 1);
		}
		startIndex = path.IndexOf("[");
		endIndex = path.IndexOf("]");
		return path;
	}

	internal void UpdateMappedValue(InlineContentControl inlineControl, WordDocument document, string replacementText)
	{
		PartContainer customXMLContainer = document.CustomXMLContainer;
		bool flag = false;
		if (customXMLContainer != null)
		{
			foreach (KeyValuePair<string, Part> xmlPart in customXMLContainer.XmlParts)
			{
				string key = xmlPart.Key;
				if (key.Contains("Props") && customXMLContainer.XmlParts[xmlPart.Key].DataStream.Length > 0)
				{
					System.Xml.Linq.XAttribute xAttribute = null;
					xAttribute = customXMLContainer.XmlParts[xmlPart.Key].GetXMLAttribute("itemID");
					if (xAttribute != null && xAttribute.Value.Equals(inlineControl.ContentControlProperties.XmlMapping.StoreItemID))
					{
						flag = true;
						ReplaceMappedValue(document, inlineControl, customXMLContainer, key, replacementText);
						break;
					}
				}
			}
		}
		if (!flag)
		{
			ReplaceMappedValue(document, inlineControl, customXMLContainer, null, replacementText);
		}
	}

	internal void UpdateMappedValue(BlockContentControl blockControl, WordDocument document, string replacementText)
	{
		PartContainer customXMLContainer = document.CustomXMLContainer;
		bool flag = false;
		if (customXMLContainer != null)
		{
			foreach (KeyValuePair<string, Part> xmlPart in customXMLContainer.XmlParts)
			{
				string key = xmlPart.Key;
				if (key.Contains("Props") && customXMLContainer.XmlParts[xmlPart.Key].DataStream.Length > 0)
				{
					System.Xml.Linq.XAttribute xAttribute = null;
					xAttribute = customXMLContainer.XmlParts[xmlPart.Key].GetXMLAttribute("itemID");
					if (xAttribute != null && xAttribute.Value.Equals(blockControl.ContentControlProperties.XmlMapping.StoreItemID))
					{
						flag = true;
						ReplaceMappedValue(document, blockControl, customXMLContainer, key, replacementText);
						break;
					}
				}
			}
		}
		if (!flag)
		{
			ReplaceMappedValue(document, blockControl, customXMLContainer, null, replacementText);
		}
	}

	private void ReplaceMappedValue(WordDocument document, InlineContentControl inlineControl, PartContainer partContainer, string mappedXmlPart, string replacementText)
	{
		bool isRootElement = false;
		bool isDocProperty = false;
		bool isMappedElementHasNoValue = false;
		if (mappedXmlPart != null)
		{
			foreach (string key in partContainer.XmlParts.Keys)
			{
				if (mappedXmlPart.Replace("Props", "") == key || partContainer.XmlParts.Keys.Count % 2 != 0 || mappedXmlPart == "")
				{
					XmlElement xmlElement = GetXmlElement(key, partContainer);
					XmlDocument ownerDocument = xmlElement.OwnerDocument;
					GetOrUpdateMappedValue(document, inlineControl, xmlElement, replacementText, isMappedwithCustomXML: true, ref isRootElement, ref isDocProperty, ref isMappedElementHasNoValue);
					if (!string.IsNullOrEmpty(inlineControl.ContentControlProperties.XmlMapping.XPath))
					{
						SaveXmlElement(key, partContainer, ownerDocument);
					}
				}
			}
			return;
		}
		GetOrUpdateMappedValue(document, inlineControl, null, replacementText, isMappedwithCustomXML: false, ref isRootElement, ref isDocProperty, ref isMappedElementHasNoValue);
	}

	private void ReplaceMappedValue(WordDocument document, BlockContentControl blockControl, PartContainer partContainer, string mappedXmlPart, string replacementText)
	{
		bool isRootElement = false;
		bool isDocProperty = false;
		bool isMappedElementHasNoValue = false;
		if (mappedXmlPart != null)
		{
			foreach (string key in partContainer.XmlParts.Keys)
			{
				if (mappedXmlPart.Replace("Props", "") == key || partContainer.XmlParts.Keys.Count % 2 != 0 || mappedXmlPart == "")
				{
					XmlElement xmlElement = GetXmlElement(key, partContainer);
					XmlDocument ownerDocument = xmlElement.OwnerDocument;
					GetOrUpdateMappedValue(document, blockControl, xmlElement, replacementText, ref isRootElement, ref isDocProperty, ref isMappedElementHasNoValue);
					if (!string.IsNullOrEmpty(blockControl.ContentControlProperties.XmlMapping.XPath))
					{
						SaveXmlElement(key, partContainer, ownerDocument);
					}
				}
			}
			return;
		}
		GetOrUpdateMappedValue(document, blockControl, null, replacementText, ref isRootElement, ref isDocProperty, ref isMappedElementHasNoValue);
	}

	private XmlElement GetXmlElement(string xmlPartsPath, PartContainer partContainer)
	{
		if (partContainer.XmlParts[xmlPartsPath].DataStream.Length <= 0)
		{
			return null;
		}
		Stream stream = UtilityMethods.CloneStream(partContainer.XmlParts[xmlPartsPath].DataStream);
		stream.Position = 0L;
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.PreserveWhitespace = true;
		xmlDocument.Load(stream);
		return xmlDocument.DocumentElement;
	}

	private void SaveXmlElement(string xmlPartsPath, PartContainer partContainer, XmlDocument doc)
	{
		if (partContainer.XmlParts[xmlPartsPath].DataStream.Length > 0)
		{
			MemoryStream memoryStream = new MemoryStream();
			doc.Save(memoryStream);
			memoryStream.Flush();
			memoryStream.Position = 0L;
			partContainer.XmlParts[xmlPartsPath].SetDataStream(memoryStream);
		}
	}
}
