using System;

namespace DocGen.Pdf;

public class PdfAnnotationException : PdfDocumentException
{
	private const string ErrorMessage = "Annotation exception.";

	public PdfAnnotationException()
		: this("Annotation exception.")
	{
	}

	public PdfAnnotationException(Exception innerException)
		: this("Annotation exception.", innerException)
	{
	}

	public PdfAnnotationException(string message)
		: base(message)
	{
	}

	public PdfAnnotationException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
