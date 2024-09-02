using System.Xml;

namespace DocGen.DocIO.DLS.XML;

public interface IXDLSContentWriter
{
	XmlWriter InnerWriter { get; }

	void WriteChildBinaryElement(string name, byte[] value);

	void WriteChildStringElement(string name, string value);

	void WriteChildElement(string name, object value);

	void WriteChildRefElement(string name, int refToElement);
}
