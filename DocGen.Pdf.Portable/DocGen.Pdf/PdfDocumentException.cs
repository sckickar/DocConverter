using System;

namespace DocGen.Pdf;

public class PdfDocumentException : PdfException
{
	private const string ErrorMessage = "Critical error on the document level.";

	public PdfDocumentException()
		: this("Critical error on the document level.")
	{
	}

	public PdfDocumentException(Exception innerException)
		: this("Critical error on the document level.", innerException)
	{
	}

	public PdfDocumentException(string message)
		: base(message)
	{
	}

	public PdfDocumentException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
