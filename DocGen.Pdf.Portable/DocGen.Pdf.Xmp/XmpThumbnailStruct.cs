using System;
using System.Xml.Linq;

namespace DocGen.Pdf.Xmp;

public class XmpThumbnailStruct : XmpStructure
{
	private const string c_prefix = "xapG";

	private const string c_name = "http://ns.adobe.com/xap/1.0/g/img/";

	private const string c_height = "height";

	private const string c_width = "width";

	private const string c_format = "format";

	private const string c_image = "image";

	protected override string StructurePrefix => "xapG";

	protected override string StructureURI => "http://ns.adobe.com/xap/1.0/g/img/";

	public float Height
	{
		get
		{
			return GetSimpleProperty("height").GetReal();
		}
		set
		{
			GetSimpleProperty("height").SetReal(value);
		}
	}

	public float Width
	{
		get
		{
			return GetSimpleProperty("width").GetReal();
		}
		set
		{
			GetSimpleProperty("width").SetReal(value);
		}
	}

	public string Format
	{
		get
		{
			return GetSimpleProperty("format").Value;
		}
		set
		{
			if (Format == null)
			{
				throw new ArgumentNullException("format");
			}
			GetSimpleProperty("format").Value = value;
		}
	}

	public byte[] Image
	{
		get
		{
			return Convert.FromBase64String(GetSimpleProperty("image").Value);
		}
		set
		{
			if (Image == null)
			{
				throw new ArgumentNullException("Image");
			}
			GetSimpleProperty("image").Value = Convert.ToBase64String(value);
		}
	}

	internal XmpThumbnailStruct(XmpMetadata xmp, XNode parent, string prefix, string localName, string namespaceURI, bool insideArray)
		: base(xmp, parent, prefix, localName, namespaceURI, insideArray)
	{
	}

	protected override void InitializeEntities()
	{
	}
}
