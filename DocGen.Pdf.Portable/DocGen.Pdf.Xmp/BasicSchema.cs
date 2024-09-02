using System;

namespace DocGen.Pdf.Xmp;

public class BasicSchema : XmpSchema
{
	private const string c_prefix = "xap";

	private const string c_name = "http://ns.adobe.com/xap/1.0/";

	private const string c_propAdvisory = "Advisory";

	private const string c_propIdentifier = "Identifier";

	private const string c_propLabel = "Label";

	private const string c_propNickname = "Nickname";

	private const string c_propBaseUrl = "BaseURL";

	private const string c_propCreatorTool = "CreatorTool";

	private const string c_propCreateData = "CreateDate";

	private const string c_propMetadataDate = "MetadataDate";

	private const string c_propModifyDate = "ModifyDate";

	private const string c_propThumbnail = "Thumbnails";

	private const string c_propRating = "Rating";

	internal bool m_externalCreationDate;

	internal bool m_externalModifyDate;

	public override XmpSchemaType SchemaType => XmpSchemaType.BasicSchema;

	protected override string Name => "http://ns.adobe.com/xap/1.0/";

	protected override string Prefix => "xap";

	public XmpArray Advisory => GetArray("Advisory", XmpArrayType.Bag);

	public XmpArray Identifier => GetArray("Identifier", XmpArrayType.Bag);

	public string Label
	{
		get
		{
			return GetSimpleProperty("Label").Value;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Label");
			}
			GetSimpleProperty("Label").Value = value;
		}
	}

	public string Nickname
	{
		get
		{
			return GetSimpleProperty("Nickname").Value;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Nickname");
			}
			GetSimpleProperty("Nickname").Value = value;
		}
	}

	public Uri BaseURL
	{
		get
		{
			return GetSimpleProperty("BaseURL").GetUri();
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("BaseURL");
			}
			GetSimpleProperty("BaseURL").SetUri(value);
		}
	}

	public string CreatorTool
	{
		get
		{
			return GetSimpleProperty("CreatorTool").Value;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("CreatorTool");
			}
			GetSimpleProperty("CreatorTool").Value = value;
		}
	}

	public DateTime CreateDate
	{
		get
		{
			return GetSimpleProperty("CreateDate").GetDateTime();
		}
		set
		{
			GetSimpleProperty("CreateDate").SetDateTime(value);
			m_externalCreationDate = true;
		}
	}

	public DateTime MetadataDate
	{
		get
		{
			return GetSimpleProperty("MetadataDate").GetDateTime();
		}
		set
		{
			GetSimpleProperty("MetadataDate").SetDateTime(value);
		}
	}

	public DateTime ModifyDate
	{
		get
		{
			return GetSimpleProperty("ModifyDate").GetDateTime();
		}
		set
		{
			GetSimpleProperty("ModifyDate").SetDateTime(value);
			m_externalModifyDate = true;
		}
	}

	public XmpArray Thumbnails => GetArray("Thumbnails", XmpArrayType.Alt);

	public XmpArray Rating => GetArray("Rating", XmpArrayType.Bag);

	protected internal BasicSchema(XmpMetadata xmp)
		: base(xmp)
	{
	}
}
