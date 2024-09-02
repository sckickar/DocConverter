using System;

namespace DocGen.Pdf.Xmp;

public class PDFSchema : XmpSchema
{
	private const string c_prefix = "pdf";

	private const string c_name = "http://ns.adobe.com/pdf/1.3/";

	private const string c_Keywords = "Keywords";

	private const string c_PDFVersion = "PDFVersion";

	private const string c_Producer = "Producer";

	public override XmpSchemaType SchemaType => XmpSchemaType.PDFSchema;

	protected override string Name => "http://ns.adobe.com/pdf/1.3/";

	protected override string Prefix => "pdf";

	public string Keywords
	{
		get
		{
			return GetSimpleProperty("Keywords").Value;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Keywords");
			}
			GetSimpleProperty("Keywords").Value = value;
		}
	}

	public string PDFVersion
	{
		get
		{
			return GetSimpleProperty("PDFVersion").Value;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("PDFVersion");
			}
			GetSimpleProperty("PDFVersion").Value = value;
		}
	}

	public string Producer
	{
		get
		{
			return GetSimpleProperty("Producer").Value;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Producer");
			}
			GetSimpleProperty("Producer").Value = value;
		}
	}

	protected internal PDFSchema(XmpMetadata xmp)
		: base(xmp)
	{
	}
}
