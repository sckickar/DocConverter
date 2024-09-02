using System;
using System.Xml.Linq;

namespace DocGen.Pdf.Xmp;

public class XmpDimensionsStruct : XmpStructure
{
	private const string c_prefix = "stDim";

	private const string c_name = "http://ns.adobe.com/xap/1.0/sType/Dimensions#";

	private const string c_width = "w";

	private const string c_height = "h";

	private const string c_unit = "unit";

	public float Width
	{
		get
		{
			return GetSimpleProperty("w").GetReal();
		}
		set
		{
			GetSimpleProperty("w").SetReal(value);
		}
	}

	public float Height
	{
		get
		{
			return GetSimpleProperty("h").GetReal();
		}
		set
		{
			GetSimpleProperty("h").SetReal(value);
		}
	}

	public string Unit
	{
		get
		{
			return GetSimpleProperty("unit").Value;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Unit");
			}
			GetSimpleProperty("unit").Value = value;
		}
	}

	protected override string StructurePrefix => "stDim";

	protected override string StructureURI => "http://ns.adobe.com/xap/1.0/sType/Dimensions#";

	internal XmpDimensionsStruct(XmpMetadata xmp, XNode parent, string prefix, string localName, string namespaceURI, bool insideArray)
		: base(xmp, parent, prefix, localName, namespaceURI, insideArray)
	{
	}

	protected override void InitializeEntities()
	{
	}
}
