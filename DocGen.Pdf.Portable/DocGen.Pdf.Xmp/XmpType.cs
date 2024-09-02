using System;
using System.Xml.Linq;

namespace DocGen.Pdf.Xmp;

public abstract class XmpType : XmpEntityBase
{
	private XmpMetadata m_xmp;

	protected internal XmpMetadata Xmp => m_xmp;

	internal XmpType(XmpMetadata xmp, XNode parent, string prefix, string localName, string namespaceURI)
		: base(parent, prefix, localName, namespaceURI)
	{
		if (xmp == null)
		{
			throw new ArgumentNullException("xmp");
		}
		m_xmp = xmp;
		Initialize();
	}

	protected override XElement GetEntityXml()
	{
		XNode xNode = null;
		if (m_xmp.isLoadedDocument)
		{
			if (!base.EntityParent.IsEmpty)
			{
				foreach (XElement item in base.EntityParent.Descendants())
				{
					if (item.Name.LocalName == base.EntityName)
					{
						xNode = item;
						break;
					}
				}
			}
		}
		else
		{
			foreach (XElement item2 in base.EntityParent.Descendants())
			{
				if (item2.Name.LocalName == base.EntityName)
				{
					xNode = item2;
					break;
				}
			}
		}
		return xNode as XElement;
	}

	protected override void CreateEntity()
	{
		XElement content = Xmp.CreateElement(base.EntityPrefix, base.EntityName, base.EntityNamespaceURI);
		base.EntityParent.Add(content);
	}
}
