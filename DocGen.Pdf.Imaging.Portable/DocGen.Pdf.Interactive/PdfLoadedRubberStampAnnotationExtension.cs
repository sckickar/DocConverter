using System.IO;

namespace DocGen.Pdf.Interactive;

public static class PdfLoadedRubberStampAnnotationExtension
{
	public static Stream[] GetImages(this PdfLoadedRubberStampAnnotation annotation)
	{
		return new PdfImageExtractionHelper(annotation.Dictionary).ImageStreams;
	}
}
