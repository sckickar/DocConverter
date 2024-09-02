using System.IO;

namespace DocGen.Pdf.Interactive;

public class PdfAttachment : PdfEmbeddedFileSpecification
{
	internal PdfAttachment(string fileName)
		: base(fileName)
	{
	}

	public PdfAttachment(string fileName, byte[] data)
		: base(fileName, data)
	{
	}

	public PdfAttachment(string fileName, Stream stream)
		: base(fileName, stream)
	{
	}
}
