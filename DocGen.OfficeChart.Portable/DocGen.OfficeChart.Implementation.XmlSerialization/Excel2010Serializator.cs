using System;
using System.Xml;

namespace DocGen.OfficeChart.Implementation.XmlSerialization;

internal class Excel2010Serializator : Excel2007Serializator
{
	private const string VersionValue = "14.0300";

	public const string DataBarUri = "{B025F937-C7B1-47D3-B67F-A62EFF666E3E}";

	public const string DataBarExtUri = "{78C0D931-6437-407d-A8EE-F0AAD7539E65}";

	public override OfficeVersion Version => OfficeVersion.Excel2010;

	public Excel2010Serializator(WorkbookImpl book)
		: base(book)
	{
	}

	protected override void SerilaizeExtensions(XmlWriter writer, WorksheetImpl sheet)
	{
	}

	public void SerializeSparklineGroups(XmlWriter writer, WorksheetImpl sheet)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		writer.WriteStartElement("extLst", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");
		writer.WriteStartElement("ext", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");
		writer.WriteAttributeString("xmlns", "x14", null, "http://schemas.microsoft.com/office/spreadsheetml/2009/9/main");
		writer.WriteAttributeString("xmlns", "xm", null, "http://schemas.microsoft.com/office/excel/2006/main");
		writer.WriteEndElement();
		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	protected override void SerializeAppVersion(XmlWriter writer)
	{
		Excel2007Serializator.SerializeElementString(writer, "AppVersion", "14.0300", null);
	}

	public new void SerializeRgbColor(XmlWriter writer, string tagName, ChartColor color)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (tagName == null || tagName.Length == 0)
		{
			throw new ArgumentOutOfRangeException("tagName");
		}
		int value = color.Value;
		if (tagName != null)
		{
			writer.WriteStartElement("x14", tagName, null);
			writer.WriteAttributeString("rgb", value.ToString("X8"));
			Excel2007Serializator.SerializeAttribute(writer, "tint", color.Tint, 0.0);
			writer.WriteEndElement();
		}
	}
}
