using System.IO;
using DocGen.Pdf.Parsing;

namespace DocGen.Pdf.Interactive;

public static class PdfLoadedSignatureFieldExtension
{
	public static Stream[] GetImages(this PdfLoadedSignatureField signatureField)
	{
		return new PdfImageExtractionHelper(signatureField.Dictionary).ImageStreams;
	}
}
