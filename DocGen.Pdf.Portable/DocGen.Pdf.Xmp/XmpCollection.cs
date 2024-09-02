using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace DocGen.Pdf.Xmp;

public abstract class XmpCollection : XmpType
{
	protected const string c_itemName = "li";

	public int Count => GetItemsCount();

	protected abstract XmpArrayType ArrayType { get; }

	protected XElement ItemsContainer
	{
		get
		{
			string elmName = GetArrayName();
			return (base.XmlData.Descendants().SingleOrDefault((XElement p) => p.Name.LocalName == elmName) ?? throw new ArgumentNullException("node")) as XElement;
		}
	}

	internal XmpCollection(XmpMetadata xmp, XNode parent, string prefix, string localName, string namespaceURI)
		: base(xmp, parent, prefix, localName, namespaceURI)
	{
	}

	protected override void CreateEntity()
	{
		if (ArrayType != 0)
		{
			base.CreateEntity();
			string arrayName = GetArrayName();
			XElement content = base.Xmp.CreateElement("rdf", arrayName, "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
			base.XmlData.Add(content);
		}
	}

	protected IEnumerable<XElement> GetArrayItems()
	{
		string text = "./rdf:li";
		return ItemsContainer.Descendants(text);
	}

	private string GetArrayName()
	{
		string result = XmpArrayType.Bag.ToString();
		if (base.XmlData.Value.Contains("rdf:Seq"))
		{
			result = XmpArrayType.Seq.ToString();
		}
		else if (base.XmlData.Value.Contains("rdf:Alt"))
		{
			result = XmpArrayType.Alt.ToString();
		}
		else if (base.XmlData.ToString().Contains("rdf:Seq"))
		{
			result = XmpArrayType.Seq.ToString();
		}
		else if (base.XmlData.ToString().Contains("rdf:Alt"))
		{
			result = XmpArrayType.Alt.ToString();
		}
		return result;
	}

	private int GetItemsCount()
	{
		return GetArrayItems().ToList().Count;
	}
}
