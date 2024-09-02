using System;

namespace DocGen.Pdf.Xmp;

public class DublinCoreSchema : XmpSchema
{
	private const string c_prefix = "dc";

	private const string c_name = "http://purl.org/dc/elements/1.1/";

	private const string c_coverage = "coverage";

	private const string c_identifier = "identifier";

	private const string c_format = "format";

	private const string c_source = "source";

	private const string c_subject = "subject";

	private const string c_type = "type";

	private const string c_contributor = "contributor";

	private const string c_creator = "creator";

	private const string c_date = "date";

	private const string c_publisher = "publisher";

	private const string c_relation = "relation";

	private const string c_description = "description";

	private const string c_rights = "rights";

	private const string c_title = "title";

	private const string c_mimeType = "application/pdf";

	public override XmpSchemaType SchemaType => XmpSchemaType.DublinCoreSchema;

	protected override string Name => "http://purl.org/dc/elements/1.1/";

	protected override string Prefix => "dc";

	public XmpArray Contributor => GetArray("contributor", XmpArrayType.Bag);

	public string Coverage
	{
		get
		{
			return GetSimpleProperty("coverage").Value;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Coverage");
			}
			GetSimpleProperty("coverage").Value = value;
		}
	}

	public XmpArray Creator
	{
		get
		{
			if (base.XmlData.Value.Contains("rdf:Bag"))
			{
				return GetArray("creator", XmpArrayType.Bag);
			}
			return GetArray("creator", XmpArrayType.Seq);
		}
	}

	public XmpArray Date => GetArray("date", XmpArrayType.Seq);

	public XmpLangArray Description => GetLangArray("description");

	public string Identifier
	{
		get
		{
			return GetSimpleProperty("identifier").Value;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Identifier");
			}
			GetSimpleProperty("identifier").Value = value;
		}
	}

	public XmpArray Publisher => GetArray("publisher", XmpArrayType.Bag);

	public XmpArray Relation => GetArray("relation", XmpArrayType.Bag);

	public XmpLangArray Rights => GetLangArray("rights");

	public string Source
	{
		get
		{
			return GetSimpleProperty("source").Value;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Source");
			}
			GetSimpleProperty("source").Value = value;
		}
	}

	public XmpArray Sublect => GetArray("subject", XmpArrayType.Bag);

	public XmpLangArray Title => GetLangArray("title");

	public XmpArray Type => GetArray("type", XmpArrayType.Bag);

	protected internal DublinCoreSchema(XmpMetadata xmp)
		: base(xmp)
	{
	}

	protected override void CreateEntity()
	{
		base.CreateEntity();
		GetSimpleProperty("format").Value = "application/pdf";
	}
}
