using System;
using System.Xml.Linq;

namespace DocGen.Pdf.Xmp;

public class XmpFontStruct : XmpStructure
{
	private const string c_prefix = "stFnt";

	private const string c_name = "http:ns.adobe.com/xap/1.0/sType/Font#";

	private const string c_fontName = "fontName";

	private const string c_fontFamily = "fontFamily";

	private const string c_fontFace = "fontFace";

	private const string c_fontType = "fontType";

	private const string c_versionString = "versionString";

	private const string c_composite = "composite";

	private const string c_fontFileName = "fontFileName";

	private const string c_childFontFiles = "childFontFiles";

	protected override string StructurePrefix => "stFnt";

	protected override string StructureURI => "http:ns.adobe.com/xap/1.0/sType/Font#";

	public string FontName
	{
		get
		{
			return GetSimpleProperty("fontName").Value;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("fontName");
			}
			GetSimpleProperty("fontName").Value = value;
		}
	}

	public string FontFamily
	{
		get
		{
			return GetSimpleProperty("fontFamily").Value;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("fontFamily");
			}
			GetSimpleProperty("fontFamily").Value = value;
		}
	}

	public string FontFace
	{
		get
		{
			return GetSimpleProperty("fontFace").Value;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("fontFace");
			}
			GetSimpleProperty("fontFace").Value = value;
		}
	}

	public string FontType
	{
		get
		{
			return GetSimpleProperty("fontType").Value;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("fontType");
			}
			GetSimpleProperty("fontType").Value = value;
		}
	}

	public string VersionString
	{
		get
		{
			return GetSimpleProperty("versionString").Value;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("versionString");
			}
			GetSimpleProperty("versionString").Value = value;
		}
	}

	public bool Composite
	{
		get
		{
			return GetSimpleProperty("composite").GetBool();
		}
		set
		{
			GetSimpleProperty("composite").SetBool(value);
		}
	}

	public string FontFileName
	{
		get
		{
			return GetSimpleProperty("fontFileName").Value;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("fontFileName");
			}
			GetSimpleProperty("fontFileName").Value = value;
		}
	}

	public XmpArray ChildFontFiles => GetArray("childFontFiles", XmpArrayType.Seq);

	internal XmpFontStruct(XmpMetadata xmp, XNode parent, string prefix, string localName, string namespaceURI, bool insideArray)
		: base(xmp, parent, prefix, localName, namespaceURI, insideArray)
	{
	}

	protected override void InitializeEntities()
	{
	}
}
