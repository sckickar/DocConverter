using System.Collections.Generic;
using System.Xml;

namespace DocGen.DocIO.DLS;

internal class XNode
{
	private XNode m_parentNode;

	private string m_name;

	private string m_value;

	private List<XNode> m_childNodes = new List<XNode>();

	private List<XAttribute> m_attributes = new List<XAttribute>();

	private XmlNodeType m_nodeType;

	internal XmlNodeType NodeType
	{
		get
		{
			return m_nodeType;
		}
		set
		{
			m_nodeType = value;
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

	internal string LocalName
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

	internal string Value
	{
		get
		{
			return m_value;
		}
		set
		{
			m_value = value;
		}
	}

	internal string InnerText
	{
		get
		{
			return m_value;
		}
		set
		{
			m_value = value;
		}
	}

	internal XNode ParentNode
	{
		get
		{
			return m_parentNode;
		}
		set
		{
			m_parentNode = value;
		}
	}

	internal XNode NextSibling
	{
		get
		{
			int num = m_parentNode.ChildNodes.IndexOf(this);
			if (num == m_parentNode.ChildNodes.Count - 1)
			{
				return null;
			}
			return m_parentNode.ChildNodes[num + 1];
		}
	}

	internal XNode PreviousSibling
	{
		get
		{
			int num = m_parentNode.ChildNodes.IndexOf(this);
			if (num == 0)
			{
				return null;
			}
			return m_parentNode.ChildNodes[num - 1];
		}
	}

	internal List<XNode> ChildNodes
	{
		get
		{
			return m_childNodes;
		}
		set
		{
			m_childNodes = value;
		}
	}

	internal List<XAttribute> Attributes
	{
		get
		{
			return m_attributes;
		}
		set
		{
			m_attributes = value;
		}
	}
}
