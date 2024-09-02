namespace DocGen.Pdf;

public class PdfInvalidPasswordException : PdfException
{
	internal PdfInvalidPasswordException()
	{
	}

	internal PdfInvalidPasswordException(string message)
		: base(message)
	{
	}
}
