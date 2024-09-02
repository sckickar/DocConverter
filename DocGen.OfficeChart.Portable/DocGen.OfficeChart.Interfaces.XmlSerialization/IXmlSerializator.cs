using System.Xml;

namespace DocGen.OfficeChart.Interfaces.XmlSerialization;

internal interface IXmlSerializator
{
	void Serialize(XmlWriter writer, IWorkbook book);
}
