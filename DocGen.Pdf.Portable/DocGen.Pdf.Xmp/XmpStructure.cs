using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace DocGen.Pdf.Xmp;

public abstract class XmpStructure : XmpType
{
	private Dictionary<string, object> m_properties;

	private bool m_bInsideArray;

	private bool m_bSuspend = true;

	private bool m_bInitialized;

	protected internal XElement InnerXmlData
	{
		get
		{
			XElement xElement = null;
			xElement = ((!m_bInsideArray) ? GetDescriptionElement() : base.XmlData);
			if (xElement == null)
			{
				throw new ArgumentNullException("elm");
			}
			return xElement;
		}
	}

	protected abstract string StructurePrefix { get; }

	protected abstract string StructureURI { get; }

	internal XmpStructure(XmpMetadata xmp, XNode parent, string prefix, string localName, string namespaceURI)
		: this(xmp, parent, prefix, localName, namespaceURI, insideArray: false)
	{
	}

	internal XmpStructure(XmpMetadata xmp, XNode parent, string prefix, string localName, string namespaceURI, bool insideArray)
		: base(xmp, parent, prefix, localName, namespaceURI)
	{
		m_bInsideArray = insideArray;
		m_bSuspend = false;
		m_properties = new Dictionary<string, object>();
		Initialize();
	}

	protected override void CreateEntity()
	{
		if (m_properties != null)
		{
			base.Xmp.AddNamespace(StructurePrefix, StructureURI);
			if (!m_bInsideArray)
			{
				base.CreateEntity();
			}
			CreateStructureContent();
			InitializeEntities();
			m_bInitialized = true;
		}
	}

	protected override XElement GetEntityXml()
	{
		XElement xElement = null;
		xElement = (m_bInsideArray ? GetDescriptionElement() : base.GetEntityXml());
		if (xElement == null)
		{
			throw new ArgumentNullException("elm");
		}
		return xElement;
	}

	protected override bool GetSuspend()
	{
		return m_bSuspend;
	}

	protected override bool CheckIfExists()
	{
		bool result = false;
		if (m_bInitialized)
		{
			result = base.CheckIfExists();
		}
		return result;
	}

	protected abstract void InitializeEntities();

	protected XmpSimpleType CreateSimpleProperty(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		return CreateSimpleProperty(name, InnerXmlData);
	}

	protected XmpSimpleType GetSimpleProperty(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		XmpSimpleType xmpSimpleType = null;
		if (m_properties.ContainsKey(name))
		{
			xmpSimpleType = m_properties[name] as XmpSimpleType;
		}
		if (xmpSimpleType == null)
		{
			xmpSimpleType = CreateSimpleProperty(name);
			m_properties[name] = xmpSimpleType;
		}
		return xmpSimpleType;
	}

	protected XmpSimpleType CreateSimpleProperty(string name, XNode parent)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		return new XmpSimpleType(base.Xmp, parent, StructurePrefix, name, StructureURI);
	}

	protected XmpSimpleType GetSimpleProperty(string name, XNode parent)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		XmpSimpleType xmpSimpleType = m_properties[name] as XmpSimpleType;
		if (xmpSimpleType == null)
		{
			xmpSimpleType = CreateSimpleProperty(name, parent);
			m_properties[name] = xmpSimpleType;
		}
		return xmpSimpleType;
	}

	protected XmpArray CreateArray(string name, XmpArrayType arrayType)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (arrayType == XmpArrayType.Unknown)
		{
			throw new ArgumentException("Wrong array type", "arrayType");
		}
		return new XmpArray(base.Xmp, InnerXmlData, StructurePrefix, name, StructureURI, arrayType);
	}

	protected XmpArray GetArray(string name, XmpArrayType arrayType)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		XmpArray xmpArray = m_properties[name] as XmpArray;
		if (xmpArray == null)
		{
			xmpArray = CreateArray(name, arrayType);
			m_properties[name] = xmpArray;
		}
		return xmpArray;
	}

	protected void CreateStructureContent()
	{
		XElement contentParent = GetContentParent();
		XElement xElement = base.Xmp.CreateElement("rdf", "Description", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
		contentParent.Add(xElement);
		XAttribute content = base.Xmp.CreateAttribute(StructurePrefix, StructureURI);
		xElement.Add(content);
	}

	private XElement GetDescriptionElement()
	{
		GetContentParent();
		return base.EntityParent.Descendants().SingleOrDefault((XElement p) => p.Name.LocalName == "Description");
	}

	private XElement GetContentParent()
	{
		if (!m_bInsideArray)
		{
			return base.XmlData;
		}
		return base.EntityParent;
	}
}
