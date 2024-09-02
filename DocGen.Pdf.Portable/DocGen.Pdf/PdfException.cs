using System;

namespace DocGen.Pdf;

public class PdfException : Exception
{
	public PdfException()
	{
	}

	public PdfException(string message)
		: base(message)
	{
	}

	public PdfException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
