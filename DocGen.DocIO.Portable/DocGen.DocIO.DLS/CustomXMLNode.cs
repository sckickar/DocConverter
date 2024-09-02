using System.Collections.Generic;
using System.Xml.Linq;

namespace DocGen.DocIO.DLS;

public class CustomXMLNode
{
	private IEnumerable<CustomXMLNode> m_childNodes;

	private CustomXMLNode m_firstChild;

	private CustomXMLNode m_lastChild;

	private string m_text;

	private CustomXMLPart m_ownerPart;

	private CustomXMLNode m_parentNode;

	private string m_xml;

	private string m_xPath;

	public string XML
	{
		get
		{
			return m_xml;
		}
		internal set
		{
			m_xml = value;
			System.Xml.Linq.XDocument xDocument = System.Xml.Linq.XDocument.Parse(value);
			m_text = xDocument.ToString();
			System.Xml.Linq.XNode firstNode = xDocument.FirstNode;
			m_firstChild = new CustomXMLNode();
			m_firstChild.m_xml = firstNode.ToString();
			m_firstChild.OwnerPart = OwnerPart;
			m_childNodes = AddChildNodes(firstNode);
			firstNode = xDocument.LastNode;
			m_lastChild = new CustomXMLNode();
			m_lastChild.m_xml = firstNode.ToString();
			m_lastChild.OwnerPart = OwnerPart;
		}
	}

	public IEnumerable<CustomXMLNode> ChildNodes => m_childNodes;

	public string XPath
	{
		get
		{
			return m_xPath;
		}
		internal set
		{
			m_xPath = value;
		}
	}

	public CustomXMLNode FirstChild => m_firstChild;

	public CustomXMLNode LastChild => m_lastChild;

	public string Text
	{
		get
		{
			return m_text;
		}
		set
		{
			m_text = value;
			System.Xml.Linq.XDocument xDocument = System.Xml.Linq.XDocument.Parse(OwnerPart.XML);
			System.Xml.Linq.XNode xNode = xDocument.SelectSingleNode(XPath);
			if (xNode is XElement)
			{
				((XElement)xNode).SetValue(value);
			}
			OwnerPart.XML = xDocument.ToString();
			XML = xNode.ToString();
		}
	}

	public CustomXMLPart OwnerPart
	{
		get
		{
			return m_ownerPart;
		}
		internal set
		{
			m_ownerPart = value;
		}
	}

	public CustomXMLNode ParentNode
	{
		get
		{
			System.Xml.Linq.XNode xNode = System.Xml.Linq.XDocument.Parse(OwnerPart.XML).SelectSingleNode(XPath);
			if (xNode == null || xNode.Parent == null)
			{
				return null;
			}
			new CustomXMLNode
			{
				OwnerPart = OwnerPart,
				XML = xNode.Parent.ToString()
			};
			return m_parentNode;
		}
	}

	private IEnumerable<CustomXMLNode> AddChildNodes(System.Xml.Linq.XNode nodes)
	{
		List<CustomXMLNode> list = null;
		if (nodes is XElement)
		{
			list = new List<CustomXMLNode>();
			foreach (XElement item in ((XElement)nodes).Elements())
			{
				if (item != null)
				{
					CustomXMLNode customXMLNode = new CustomXMLNode();
					XElement xElement = item;
					customXMLNode.XML = xElement.ToString();
					list.Add(customXMLNode);
				}
			}
		}
		return list;
	}

	internal CustomXMLNode()
	{
	}

	public void Delete()
	{
		System.Xml.Linq.XNode xNode = null;
		System.Xml.Linq.XDocument xDocument = System.Xml.Linq.XDocument.Parse(OwnerPart.XML);
		xNode = xDocument.SelectSingleNode(XPath);
		if (xNode != null)
		{
			xNode.Remove();
			OwnerPart.XML = xDocument.ToString();
			OwnerPart = null;
		}
	}

	public CustomXMLNode SelectSingleNode(string xPath)
	{
		System.Xml.Linq.XNode xNode = null;
		xNode = System.Xml.Linq.XDocument.Parse(XML).SelectSingleNode(XPath);
		if (xNode == null)
		{
			return null;
		}
		return new CustomXMLNode
		{
			OwnerPart = OwnerPart,
			XPath = xPath,
			XML = xNode.ToString()
		};
	}

	public bool HasChildNodes()
	{
		return m_childNodes.GetEnumerator().MoveNext();
	}

	public void AppendChildNode(string name, CustomXMLNodeType nodeType, string nodeValue)
	{
		System.Xml.Linq.XDocument xDocument = System.Xml.Linq.XDocument.Parse(OwnerPart.XML);
		System.Xml.Linq.XNode xNode = xDocument.SelectSingleNode(XPath);
		System.Xml.Linq.XNode xNode2 = null;
		if (nodeType == CustomXMLNodeType.Element)
		{
			xNode2 = new XElement(name, nodeValue);
		}
		else if (nodeType == CustomXMLNodeType.Document)
		{
			xNode2 = new System.Xml.Linq.XDocument(name);
		}
		else if (nodeType != CustomXMLNodeType.Attribute || !(xNode is XElement))
		{
			xNode2 = ((nodeType != CustomXMLNodeType.Text) ? ((System.Xml.Linq.XNode)new XElement(name)) : ((System.Xml.Linq.XNode)new XText(nodeValue)));
		}
		else
		{
			((XElement)xNode).Add(new System.Xml.Linq.XAttribute(name, nodeValue));
		}
		if (xNode2 != null)
		{
			xDocument.Add(xNode2);
			xNode.AddAfterSelf(xNode2);
		}
		XML = xNode.ToString();
		m_text = xNode.Document.Root.Value;
		OwnerPart.XML = xDocument.ToString();
	}

	public void RemoveChild(CustomXMLNode child)
	{
		System.Xml.Linq.XNode xNode = null;
		xNode = System.Xml.Linq.XDocument.Parse(XML).SelectSingleNode(child.XPath);
		if (xNode != null)
		{
			xNode.Remove();
			XML = m_xml.Replace(xNode.ToString(), "");
			if (xNode is XElement)
			{
				Text = m_text.Replace(((XElement)xNode).Value, "");
			}
		}
	}
}
