namespace DocGen.DocIO.DLS.XML;

public interface IXDLSSerializable
{
	XDLSHolder XDLSHolder { get; }

	void WriteXmlAttributes(IXDLSAttributeWriter writer);

	void WriteXmlContent(IXDLSContentWriter writer);

	void ReadXmlAttributes(IXDLSAttributeReader reader);

	bool ReadXmlContent(IXDLSContentReader reader);

	void RestoreReference(string name, int value);
}
