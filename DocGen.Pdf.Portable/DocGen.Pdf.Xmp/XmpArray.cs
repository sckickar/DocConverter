using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace DocGen.Pdf.Xmp;

public class XmpArray : XmpCollection
{
	internal const string c_dateFormat = "yyyy-MM-dd'T'HH:mm:ss.ffzzz";

	private XmpArrayType m_arrayType;

	public string[] Items => GetArrayValues();

	protected override XmpArrayType ArrayType => m_arrayType;

	internal XmpArray(XmpMetadata xmp, XNode parent, string prefix, string localName, string namespaceURI, XmpArrayType type)
		: base(xmp, parent, prefix, localName, namespaceURI)
	{
		m_arrayType = type;
		Initialize();
	}

	public void Add(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		XmpUtils.SetTextValue(CreateItem(), value);
	}

	public void Add(int value)
	{
		XmpUtils.SetIntValue(CreateItem(), value);
	}

	public void Add(float value)
	{
		XmpUtils.SetRealValue(CreateItem(), value);
	}

	public void Add(DateTime value)
	{
		string value2 = value.ToString("yyyy-MM-dd'T'HH:mm:ss.ffzzz");
		Add(value2);
	}

	public void Add(DateTime value, string format)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		string value2 = value.ToString(format);
		Add(value2);
	}

	public void Add(XmpStructure structure)
	{
		if (structure == null)
		{
			throw new ArgumentNullException("structure");
		}
		XElement parent = CreateItem();
		XmpUtils.SetXmlValue(parent, structure.XmlData);
		ChangeParent(parent, structure);
	}

	private XElement CreateItem()
	{
		if (base.ItemsContainer.FirstNode != null)
		{
			base.ItemsContainer.FirstNode.Remove();
		}
		XElement xElement = base.Xmp.CreateElement("rdf", "li", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
		base.ItemsContainer.Add(xElement);
		return xElement;
	}

	private string[] GetArrayValues()
	{
		string[] array = new string[1];
		if (base.XmlData.Value.Contains("rdf"))
		{
			List<XElement> list = GetArrayItems().ToList();
			if (list.Count == 0)
			{
				array[0] = string.Empty;
			}
			else
			{
				array = new string[list.Count];
				int i = 0;
				for (int count = list.Count; i < count; i++)
				{
					XElement xElement = list[i];
					array[i] = xElement.Value;
				}
			}
		}
		else if (base.XmlData.ToString().Contains("li"))
		{
			int num = 0;
			array = new string[(from p in base.XmlData.Descendants().Elements()
				select p.Name.LocalName == "li").Count()];
			foreach (XElement item in base.XmlData.Descendants())
			{
				if (item.Name.LocalName == "li")
				{
					array[num] = item.Value;
					num++;
				}
			}
		}
		else
		{
			array[0] = base.XmlData.Value;
		}
		return array;
	}

	private void ChangeParent(XNode parent, XmpEntityBase entity)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		if (entity == null)
		{
			throw new ArgumentNullException("entity");
		}
		entity.SetXmlParent(parent);
	}
}
