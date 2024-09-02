using System;
using System.Xml;

namespace DocGen.DocIO.DLS.XML;

public interface IXDLSContentReader
{
	string TagName { get; }

	XmlNodeType NodeType { get; }

	XmlReader InnerReader { get; }

	IXDLSAttributeReader AttributeReader { get; }

	string GetAttributeValue(string name);

	bool ReadChildElement(object value);

	object ReadChildElement(Type type);

	string ReadChildStringContent();

	byte[] ReadChildBinaryElement();
}
