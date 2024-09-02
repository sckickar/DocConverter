using System;
using System.Xml.Linq;

namespace DocGen.Pdf.Xmp;

public class XmpJobStruct : XmpStructure
{
	private const string c_prefix = "stJob";

	private const string c_structName = "http://ns.adobe.com/xap/1.0/sType/Job#";

	private const string c_name = "name";

	private const string c_id = "id";

	private const string c_url = "url";

	public string Name
	{
		get
		{
			return GetSimpleProperty("name").Value;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Name");
			}
			GetSimpleProperty("name").Value = value;
		}
	}

	public string ID
	{
		get
		{
			return GetSimpleProperty("id").Value;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("ID");
			}
			GetSimpleProperty("id").Value = value;
		}
	}

	public Uri Url
	{
		get
		{
			return GetSimpleProperty("url").GetUri();
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Url");
			}
			GetSimpleProperty("url").SetUri(value);
		}
	}

	protected override string StructurePrefix => "stJob";

	protected override string StructureURI => "http://ns.adobe.com/xap/1.0/sType/Job#";

	internal XmpJobStruct(XmpMetadata xmp, XNode parent, string prefix, string localName, string namespaceURI, bool insideArray)
		: base(xmp, parent, prefix, localName, namespaceURI, insideArray)
	{
	}

	protected override void InitializeEntities()
	{
	}
}
