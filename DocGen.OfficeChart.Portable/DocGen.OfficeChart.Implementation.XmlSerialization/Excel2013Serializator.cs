using System.Xml;

namespace DocGen.OfficeChart.Implementation.XmlSerialization;

internal class Excel2013Serializator : Excel2010Serializator
{
	private const string VersionValue = "15.0300";

	public override OfficeVersion Version => OfficeVersion.Excel2013;

	public Excel2013Serializator(WorkbookImpl book)
		: base(book)
	{
	}

	protected override void SerializeAppVersion(XmlWriter writer)
	{
		Excel2007Serializator.SerializeElementString(writer, "AppVersion", "15.0300", null);
	}
}
