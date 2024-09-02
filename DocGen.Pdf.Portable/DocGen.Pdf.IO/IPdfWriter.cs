using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.IO;

internal interface IPdfWriter
{
	long Position { get; set; }

	long Length { get; }

	PdfDocumentBase Document { get; set; }

	void Write(IPdfPrimitive pdfObject);

	void Write(long number);

	void Write(float number);

	void Write(string text);

	void Write(char[] text);

	void Write(byte[] data);
}
