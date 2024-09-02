using System.Security;
using System.Xml;

namespace DocGen.Drawing;

internal class Serializator
{
	internal Serializator()
	{
	}

	[SecurityCritical]
	internal void AddShape(ShapeImplExt shape, XmlWriter xmlTextwriter)
	{
		new AutoShapeSerializator(shape).Write(xmlTextwriter);
	}

	private void WriteHeader(XmlWriter xmlTextWriter)
	{
		xmlTextWriter.WriteStartDocument(standalone: true);
		xmlTextWriter.WriteStartElement("xdr:wsDr");
		xmlTextWriter.WriteAttributeString("xmlns", "xdr", null, "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
		xmlTextWriter.WriteAttributeString("xmlns", "a", null, "http://schemas.openxmlformats.org/drawingml/2006/main");
		xmlTextWriter.WriteAttributeString("xmlns", "r", null, "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
	}
}
