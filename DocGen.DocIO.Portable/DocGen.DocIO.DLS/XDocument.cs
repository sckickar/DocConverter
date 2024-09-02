using System.Xml;

namespace DocGen.DocIO.DLS;

internal class XDocument
{
	private XNode m_rootNode;

	public XNode RootNode
	{
		get
		{
			return m_rootNode;
		}
		set
		{
			m_rootNode = value;
		}
	}

	internal void LoadXml(XmlReader reader)
	{
		while (reader.NodeType != XmlNodeType.Element)
		{
			reader.Read();
		}
		m_rootNode = new XNode();
		m_rootNode.LocalName = reader.LocalName;
		reader.Read();
		while (reader.LocalName != m_rootNode.LocalName)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				AddNode(reader, m_rootNode, reader.NodeType);
			}
			else
			{
				reader.Read();
			}
			SkipWhitespaces(reader);
		}
	}

	private void SkipWhitespaces(XmlReader reader)
	{
		if (reader.NodeType != XmlNodeType.Element)
		{
			while (reader.NodeType == XmlNodeType.Whitespace)
			{
				reader.Read();
			}
		}
	}

	private void AddNode(XmlReader reader, XNode parent, XmlNodeType nodeType)
	{
		XNode xNode = new XNode();
		xNode.Name = reader.LocalName;
		xNode.ParentNode = parent;
		xNode.NodeType = nodeType;
		xNode.InnerText = reader.Value;
		parent?.ChildNodes.Add(xNode);
		if (reader.AttributeCount > 0)
		{
			for (int i = 0; i < reader.AttributeCount; i++)
			{
				XAttribute xAttribute = new XAttribute();
				reader.MoveToAttribute(i);
				xAttribute.Name = reader.LocalName;
				xAttribute.Value = reader.Value;
				xNode.Attributes.Add(xAttribute);
			}
			reader.MoveToElement();
		}
		xNode.InnerText = reader.Value;
		if (!reader.IsEmptyElement && reader.NodeType == XmlNodeType.Element)
		{
			string localName = reader.LocalName;
			reader.Read();
			while (reader.LocalName != localName)
			{
				AddNode(reader, xNode, reader.NodeType);
				reader.Read();
			}
		}
	}
}
