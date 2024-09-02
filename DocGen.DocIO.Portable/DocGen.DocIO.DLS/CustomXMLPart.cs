using System;
using System.IO;
using System.Xml.Linq;

namespace DocGen.DocIO.DLS;

public class CustomXMLPart
{
	private string m_id;

	private string m_xml;

	private WordDocument m_document;

	internal string XML
	{
		get
		{
			return m_xml;
		}
		set
		{
			m_xml = value;
		}
	}

	internal WordDocument Document => m_document;

	public string Id
	{
		get
		{
			return m_id;
		}
		internal set
		{
			m_id = value;
		}
	}

	internal CustomXMLPart()
	{
	}

	public CustomXMLPart(WordDocument document)
	{
		m_document = document;
	}

	public void LoadXML(string xml)
	{
		XML = xml;
		if (Id == null)
		{
			Id = Guid.NewGuid().ToString();
			m_document.CustomXmlParts.Add(Id, this);
		}
	}

	public void Load(Stream xmlStream)
	{
		if (Id == null)
		{
			Id = Guid.NewGuid().ToString();
			m_document.CustomXmlParts.Add(Id, this);
		}
		using StreamReader streamReader = new StreamReader(xmlStream);
		System.Xml.Linq.XDocument xDocument = System.Xml.Linq.XDocument.Parse(streamReader.ReadToEnd());
		XML = xDocument.ToString();
	}

	public void AddNode(CustomXMLNode customXmlNode, string name, CustomXMLNodeType nodeType, string nodeValue)
	{
		System.Xml.Linq.XDocument xDocument = System.Xml.Linq.XDocument.Parse(customXmlNode.OwnerPart.XML);
		System.Xml.Linq.XNode xNode = xDocument.SelectSingleNode(customXmlNode.XPath);
		if (nodeType != 0 && nodeType != CustomXMLNodeType.Document && nodeType != CustomXMLNodeType.Attribute)
		{
			_ = 2;
		}
		XElement content = new XElement(name);
		xNode.AddAfterSelf(content);
		XML = xDocument.ToString();
	}

	public CustomXMLNode SelectSingleNode(string xPath)
	{
		System.Xml.Linq.XNode xNode = null;
		xNode = System.Xml.Linq.XDocument.Parse(XML).SelectSingleNode(xPath);
		if (xNode == null)
		{
			return null;
		}
		return new CustomXMLNode
		{
			OwnerPart = this,
			XPath = xPath,
			XML = xNode.ToString()
		};
	}
}
