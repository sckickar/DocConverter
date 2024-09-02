namespace DocGen.Pdf.Security;

public class PdfSignatureValidationException : PdfException
{
	private PdfSignatureValidationExceptionType exceptionType;

	internal PdfSignatureValidationExceptionType ExceptionType
	{
		get
		{
			return exceptionType;
		}
		set
		{
			exceptionType = value;
		}
	}

	internal PdfSignatureValidationException()
	{
	}

	internal PdfSignatureValidationException(string message)
		: base(message)
	{
	}
}
