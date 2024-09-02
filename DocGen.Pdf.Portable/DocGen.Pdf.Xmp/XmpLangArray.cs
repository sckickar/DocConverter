using System;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace DocGen.Pdf.Xmp;

public class XmpLangArray : XmpCollection
{
	private const string c_langName = "x-default";

	private const string c_langAttribute = "lang";

	public string DefaultText
	{
		get
		{
			if (base.XmlData.Value.Contains("\t") || base.XmlData.Value.Contains("\n") || base.XmlData.Value.Contains("\r"))
			{
				base.XmlData.Value = Regex.Replace(base.XmlData.Value, "\\t|\\n|\\r", "").Trim();
			}
			if (base.XmlData.Value.Contains("rdf"))
			{
				XElement xElement = GetItem("x-default");
				if (xElement == null)
				{
					xElement = CreateItem("x-default");
				}
				return xElement.Value;
			}
			return base.XmlData.Value;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("DefaultText");
			}
			XElement xElement = GetItem("x-default");
			if (xElement == null)
			{
				xElement = CreateItem("x-default");
			}
			XmpUtils.SetTextValue(xElement, value);
		}
	}

	public string this[string lang]
	{
		get
		{
			string result = null;
			XElement item = GetItem(lang);
			if (item != null)
			{
				result = item.Value;
			}
			return result;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			XElement xElement = GetItem(lang);
			if (xElement == null)
			{
				xElement = CreateItem(lang);
			}
			XmpUtils.SetTextValue(xElement, value);
		}
	}

	protected override XmpArrayType ArrayType => XmpArrayType.Alt;

	internal XmpLangArray(XmpMetadata xmp, XNode parent, string prefix, string localName, string namespaceURI)
		: base(xmp, parent, prefix, localName, namespaceURI)
	{
	}

	public void Add(string lang, string value)
	{
		if (lang == null)
		{
			throw new ArgumentNullException("lang");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		XmpUtils.SetTextValue(CreateItem(lang), value);
	}

	protected override void CreateEntity()
	{
		base.CreateEntity();
		CreateItem("x-default");
	}

	private XElement CreateItem(string lang)
	{
		if (lang == null)
		{
			throw new ArgumentNullException("lang");
		}
		XElement xElement = base.Xmp.CreateElement("rdf", "li", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
		base.ItemsContainer.Add(xElement);
		XAttribute content = base.Xmp.CreateAttribute("xml", "lang", "http://www.w3.org/XML/1998/namespace", lang);
		xElement.Add(content);
		return xElement;
	}

	private XElement GetItem(string lang)
	{
		if (lang == null)
		{
			throw new ArgumentNullException("lang");
		}
		string text = "./rdf:li[@xml:lang=\"" + lang + "\"]";
		return base.ItemsContainer.Element(text) as XElement;
	}
}
