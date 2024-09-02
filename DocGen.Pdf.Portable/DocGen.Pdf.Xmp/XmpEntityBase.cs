using System;
using System.Xml;
using System.Xml.Linq;

namespace DocGen.Pdf.Xmp;

public abstract class XmpEntityBase
{
	private XElement m_xmlParent;

	private string m_entityPrefix;

	private string m_localName;

	private string m_namespaceURI;

	public XElement XmlData => GetEntityXml();

	protected internal bool Exists => CheckIfExists();

	protected internal XElement EntityParent => m_xmlParent;

	protected internal string EntityPrefix => m_entityPrefix;

	protected internal string EntityName => m_localName;

	protected internal string EntityNamespaceURI => m_namespaceURI;

	protected bool SuspendInitialization => GetSuspend();

	protected internal XmpEntityBase(XNode parent, string prefix, string localName, string namespaceURI)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		if (localName == null)
		{
			throw new ArgumentNullException("localName");
		}
		if (!XmlReader.IsName(localName))
		{
			localName = XmlConvert.EncodeName(localName);
		}
		m_xmlParent = parent as XElement;
		m_entityPrefix = prefix;
		m_localName = localName;
		m_namespaceURI = namespaceURI;
	}

	protected virtual void Initialize()
	{
		if (!SuspendInitialization && !Exists)
		{
			CreateEntity();
		}
	}

	protected virtual bool CheckIfExists()
	{
		return GetEntityXml() != null;
	}

	protected virtual bool GetSuspend()
	{
		return false;
	}

	protected abstract void CreateEntity();

	protected abstract XElement GetEntityXml();

	internal void SetXmlParent(XNode parent)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		m_xmlParent = parent as XElement;
	}
}
