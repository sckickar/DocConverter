namespace DocGen.Pdf.Xmp;

public class PagedTextSchema : XmpSchema
{
	private const string c_prefix = "xmpTPg";

	private const string c_name = "http://ns.adobe.com/xap/1.0/t/pg/";

	private const string c_NPages = "NPages";

	private const string c_Fonts = "Fonts";

	private const string c_PlateName = "PlateNames";

	private const string c_Colorants = "Colorants";

	private const string c_MaxPageSize = "MaxPageSize";

	public override XmpSchemaType SchemaType => XmpSchemaType.PagedTextSchema;

	protected override string Name => "http://ns.adobe.com/xap/1.0/t/pg/";

	protected override string Prefix => "xmpTPg";

	public XmpDimensionsStruct MaxPageSize => GetStructure("MaxPageSize", XmpStructureType.Dimensions) as XmpDimensionsStruct;

	public int NPages
	{
		get
		{
			return GetSimpleProperty("NPages").GetInt();
		}
		set
		{
			GetSimpleProperty("NPages").SetInt(value);
		}
	}

	public XmpArray Fonts => GetArray("Fonts", XmpArrayType.Bag);

	public XmpArray PlateNames => GetArray("PlateNames", XmpArrayType.Seq);

	public XmpArray Colorants => GetArray("Colorants", XmpArrayType.Seq);

	protected internal PagedTextSchema(XmpMetadata xmp)
		: base(xmp)
	{
	}
}
