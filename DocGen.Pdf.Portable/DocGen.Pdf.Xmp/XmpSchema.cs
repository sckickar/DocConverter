using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace DocGen.Pdf.Xmp;

public abstract class XmpSchema : XmpEntityBase
{
	internal const string c_schemaTagName = "Description";

	private const string c_xPathDescription = "/x:xmpmeta/rdf:RDF/rdf:Description";

	private XmpMetadata m_xmp;

	private Dictionary<string, object> m_properties;

	public abstract XmpSchemaType SchemaType { get; }

	protected abstract string Prefix { get; }

	protected abstract string Name { get; }

	protected internal XmpMetadata Xmp => m_xmp;

	protected internal XmpSchema(XmpMetadata xmp)
		: base(xmp.Rdf, "rdf", "Description", "http://www.w3.org/1999/02/22-rdf-syntax-ns#")
	{
		if (xmp == null)
		{
			throw new ArgumentNullException("xmp");
		}
		m_xmp = xmp;
		m_properties = new Dictionary<string, object>();
		if (Prefix != null)
		{
			Initialize();
		}
	}

	protected override void CreateEntity()
	{
		XElement xElement = Xmp.CreateElement(base.EntityPrefix, base.EntityName, base.EntityNamespaceURI);
		base.EntityParent.Add(xElement);
		XAttribute content = Xmp.CreateAttribute(base.EntityPrefix, "about", base.EntityNamespaceURI, string.Empty);
		xElement.Add(content);
		XAttribute content2 = Xmp.CreateAttribute(Prefix, Name);
		xElement.Add(content2);
		Xmp.AddNamespace(Prefix, Name);
	}

	protected override XElement GetEntityXml()
	{
		List<XNode> list = null;
		if (Xmp.NamespaceManager.HasNamespace(base.EntityPrefix))
		{
			list = base.EntityParent.Nodes().ToList();
			XNode xNode = null;
			XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
			xmlWriterSettings.CheckCharacters = false;
			StringWriter stringWriter = new StringWriter();
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				XElement xElement = list[i] as XElement;
				string text = string.Empty;
				string empty = string.Empty;
				if (xElement == null)
				{
					continue;
				}
				XElement xElement2 = null;
				foreach (XElement item in base.EntityParent.Descendants())
				{
					try
					{
						text = item.ToString();
					}
					catch (Exception)
					{
						stringWriter = new StringWriter();
						using (XmlWriter writer = XmlWriter.Create(stringWriter, xmlWriterSettings))
						{
							item.Save(writer);
						}
						text = stringWriter.ToString();
					}
					if (item.Name.LocalName == base.EntityName && text.Contains(Prefix))
					{
						xElement2 = item;
						break;
					}
				}
				try
				{
					empty = xElement.ToString();
				}
				catch (Exception)
				{
					stringWriter = new StringWriter();
					using (XmlWriter writer2 = XmlWriter.Create(stringWriter, xmlWriterSettings))
					{
						xElement.Save(writer2);
					}
					empty = stringWriter.ToString();
				}
				if (xElement2 != null && text == empty)
				{
					xNode = xElement;
					break;
				}
			}
			return xNode as XElement;
		}
		return null;
	}

	protected XmpSimpleType CreateSimpleProperty(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		return new XmpSimpleType(Xmp, base.XmlData, Prefix, name, Name);
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
		return new XmpArray(Xmp, base.XmlData, Prefix, name, Name, arrayType);
	}

	protected XmpArray GetArray(string name, XmpArrayType arrayType)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		XmpArray xmpArray = null;
		if (m_properties.ContainsKey(name))
		{
			xmpArray = m_properties[name] as XmpArray;
		}
		if (xmpArray == null)
		{
			xmpArray = CreateArray(name, arrayType);
			m_properties[name] = xmpArray;
		}
		return xmpArray;
	}

	protected XmpLangArray CreateLangArray(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		return new XmpLangArray(Xmp, base.XmlData, Prefix, name, Name);
	}

	protected XmpLangArray GetLangArray(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		XmpLangArray xmpLangArray = null;
		if (m_properties.ContainsKey(name))
		{
			xmpLangArray = m_properties[name] as XmpLangArray;
		}
		if (xmpLangArray == null)
		{
			xmpLangArray = CreateLangArray(name);
			m_properties[name] = xmpLangArray;
		}
		return xmpLangArray;
	}

	protected XmpStructure GetStructure(string name, XmpStructureType type)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		XmpStructure xmpStructure = null;
		if (m_properties.ContainsKey(name))
		{
			xmpStructure = m_properties[name] as XmpStructure;
		}
		if (xmpStructure == null)
		{
			xmpStructure = CreateStructure(name, type);
			m_properties[name] = xmpStructure;
		}
		return xmpStructure;
	}

	protected XmpStructure CreateStructure(string name, XmpStructureType type)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		XmpStructure result = null;
		bool insideArray = name.Length == 0;
		switch (type)
		{
		case XmpStructureType.Dimensions:
			result = new XmpDimensionsStruct(Xmp, base.XmlData, Prefix, name, Name, insideArray);
			break;
		case XmpStructureType.Font:
			result = new XmpFontStruct(Xmp, base.XmlData, Prefix, name, Name, insideArray);
			break;
		case XmpStructureType.Colorant:
			result = new XmpColorantStruct(Xmp, base.XmlData, Prefix, name, Name, insideArray);
			break;
		case XmpStructureType.Thumbnail:
			result = new XmpThumbnailStruct(Xmp, base.XmlData, Prefix, name, Name, insideArray);
			break;
		case XmpStructureType.Job:
			result = new XmpJobStruct(Xmp, base.XmlData, Prefix, name, Name, insideArray);
			break;
		}
		return result;
	}

	public XmpStructure CreateStructure(XmpStructureType type)
	{
		return CreateStructure(string.Empty, type);
	}
}
