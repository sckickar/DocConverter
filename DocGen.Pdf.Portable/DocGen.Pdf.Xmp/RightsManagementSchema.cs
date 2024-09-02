using System;

namespace DocGen.Pdf.Xmp;

public class RightsManagementSchema : XmpSchema
{
	private const string c_prefix = "xmpRights";

	private const string c_name = "http://ns.adobe.com/xap/1.0/rights/";

	private const string c_Certificate = "Certificate";

	private const string c_Marked = "Marked";

	private const string c_Owner = "Owner";

	private const string c_UsageTerms = "UsageTerms";

	private const string c_WebStatement = "WebStatement";

	public override XmpSchemaType SchemaType => XmpSchemaType.RightsManagementSchema;

	protected override string Name => "http://ns.adobe.com/xap/1.0/rights/";

	protected override string Prefix => "xmpRights";

	public Uri Certificate
	{
		get
		{
			return GetSimpleProperty("Certificate").GetUri();
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Certificate");
			}
			GetSimpleProperty("Certificate").SetUri(value);
		}
	}

	public bool Marked
	{
		get
		{
			return GetSimpleProperty("Marked").GetBool();
		}
		set
		{
			GetSimpleProperty("Marked").SetBool(value);
		}
	}

	public XmpArray Owner => GetArray("Owner", XmpArrayType.Bag);

	public XmpLangArray UsageTerms => GetLangArray("UsageTerms");

	public Uri WebStatement
	{
		get
		{
			return GetSimpleProperty("WebStatement").GetUri();
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("WebStatement");
			}
			GetSimpleProperty("WebStatement").SetUri(value);
		}
	}

	protected internal RightsManagementSchema(XmpMetadata xmp)
		: base(xmp)
	{
	}
}
