namespace DocGen.Pdf.Xmp;

public class BasicJobTicketSchema : XmpSchema
{
	private const string c_prefix = "xmpBJ";

	private const string c_name = "http://ns.adobe.com/xap/1.0/bj/";

	private const string c_propJobRef = "JobRef";

	public override XmpSchemaType SchemaType => XmpSchemaType.BasicJobTicketSchema;

	protected override string Name => "http://ns.adobe.com/xap/1.0/bj/";

	protected override string Prefix => "xmpBJ";

	public XmpArray JobRef => GetArray("JobRef", XmpArrayType.Bag);

	protected internal BasicJobTicketSchema(XmpMetadata xmp)
		: base(xmp)
	{
	}
}
