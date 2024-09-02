using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace DocGen.Pdf.Xmp;

public class CustomSchema : XmpSchema
{
	private string m_namespace;

	private string m_namespaceUri;

	private Dictionary<string, string> m_customdata = new Dictionary<string, string>();

	internal XmpMetadata m_Xmp;

	public string this[string name]
	{
		get
		{
			return XmlConvert.DecodeName(GetSimpleProperty(name).Value);
		}
		set
		{
			XmpSimpleType simpleProperty = GetSimpleProperty(name);
			List<string> list = new List<string> { "\v", "\f", "\r" };
			string text2 = (simpleProperty.Value = ((!value.Contains(list[0]) && !value.Contains(list[1]) && !value.Contains(list[2])) ? value : XmlConvert.EncodeLocalName(value)));
			m_customdata[name] = text2;
			if (m_Xmp.DocumentInfo != null)
			{
				m_Xmp.DocumentInfo.AddCustomMetaDataInfo(name, text2);
			}
		}
	}

	internal Dictionary<string, string> CustomData
	{
		get
		{
			return m_customdata;
		}
		set
		{
			m_customdata = value;
		}
	}

	public override XmpSchemaType SchemaType => XmpSchemaType.Custom;

	protected override string Prefix => m_namespace;

	protected override string Name => m_namespaceUri;

	internal bool ContainsKey(string key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key value should not be null");
		}
		if (m_customdata.ContainsValue(key))
		{
			return true;
		}
		return false;
	}

	public CustomSchema(XmpMetadata xmp, string xmlNamespace, string namespaceUri)
		: base(xmp)
	{
		m_Xmp = xmp;
		if (xmlNamespace == null)
		{
			throw new ArgumentNullException("xmlNamespace");
		}
		if (namespaceUri == null)
		{
			throw new ArgumentNullException("namespaceUri");
		}
		m_namespace = xmlNamespace;
		m_namespaceUri = namespaceUri;
		Initialize();
	}

	protected override XElement GetEntityXml()
	{
		XElement result = null;
		if (m_namespace != null)
		{
			result = base.GetEntityXml();
		}
		return result;
	}
}
