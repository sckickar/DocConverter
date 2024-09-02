using System;

namespace DocGen.Pdf;

public class PdfConformanceException : PdfDocumentException
{
	private const string ErrorMessage = "PDF Conformance-level exception.";

	public PdfConformanceException()
		: this("PDF Conformance-level exception.")
	{
	}

	public PdfConformanceException(Exception innerException)
		: this("PDF Conformance-level exception.", innerException)
	{
	}

	public PdfConformanceException(string message)
		: base(message)
	{
	}

	public PdfConformanceException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
