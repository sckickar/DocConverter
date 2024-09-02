using System;
using System.Xml.Linq;

namespace DocGen.Pdf.Xmp;

public class XmpColorantStruct : XmpStructure
{
	private const string c_prefix = "xapG";

	private const string c_name = "http://ns.adobe.com/xap/1.0/g/";

	private const string c_swatchName = "swatchName";

	private const string c_mode = "mode";

	private const string c_type = "type";

	private const string c_cyan = "cyan";

	private const string c_magenta = "magenta";

	private const string c_black = "black";

	private const string c_red = "red";

	private const string c_green = "green";

	private const string c_blue = "blue";

	private const string c_L = "L";

	private const string c_A = "A";

	private const string c_B = "B";

	private const string c_yellow = "yellow";

	protected override string StructurePrefix => "xapG";

	protected override string StructureURI => "http://ns.adobe.com/xap/1.0/g/";

	public float Yellow
	{
		get
		{
			return GetSimpleProperty("yellow").GetReal();
		}
		set
		{
			GetSimpleProperty("yellow").SetReal(value);
		}
	}

	public float B
	{
		get
		{
			return GetSimpleProperty("B").GetReal();
		}
		set
		{
			GetSimpleProperty("B").SetReal(value);
		}
	}

	public float A
	{
		get
		{
			return GetSimpleProperty("A").GetReal();
		}
		set
		{
			GetSimpleProperty("A").SetReal(value);
		}
	}

	public float L
	{
		get
		{
			return GetSimpleProperty("L").GetReal();
		}
		set
		{
			GetSimpleProperty("L").SetReal(value);
		}
	}

	public float Blue
	{
		get
		{
			return GetSimpleProperty("blue").GetReal();
		}
		set
		{
			GetSimpleProperty("blue").SetReal(value);
		}
	}

	public float Green
	{
		get
		{
			return GetSimpleProperty("green").GetReal();
		}
		set
		{
			GetSimpleProperty("green").SetReal(value);
		}
	}

	public float Red
	{
		get
		{
			return GetSimpleProperty("red").GetReal();
		}
		set
		{
			GetSimpleProperty("red").SetReal(value);
		}
	}

	public float Black
	{
		get
		{
			return GetSimpleProperty("black").GetReal();
		}
		set
		{
			GetSimpleProperty("black").SetReal(value);
		}
	}

	public float Magenta
	{
		get
		{
			return GetSimpleProperty("magenta").GetReal();
		}
		set
		{
			GetSimpleProperty("magenta").SetReal(value);
		}
	}

	public float Cyan
	{
		get
		{
			return GetSimpleProperty("cyan").GetReal();
		}
		set
		{
			GetSimpleProperty("cyan").SetReal(value);
		}
	}

	public string Type
	{
		get
		{
			return GetSimpleProperty("type").Value;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("type");
			}
			GetSimpleProperty("type").Value = value;
		}
	}

	public string Mode
	{
		get
		{
			return GetSimpleProperty("mode").Value;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("mode");
			}
			GetSimpleProperty("mode").Value = value;
		}
	}

	public string SwatchName
	{
		get
		{
			return GetSimpleProperty("swatchName").Value;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("swatchName");
			}
			GetSimpleProperty("swatchName").Value = value;
		}
	}

	internal XmpColorantStruct(XmpMetadata xmp, XNode parent, string prefix, string localName, string namespaceURI, bool insideArray)
		: base(xmp, parent, prefix, localName, namespaceURI, insideArray)
	{
	}

	protected override void InitializeEntities()
	{
	}
}
