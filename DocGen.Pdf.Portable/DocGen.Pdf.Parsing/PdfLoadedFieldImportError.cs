using System;

namespace DocGen.Pdf.Parsing;

public class PdfLoadedFieldImportError
{
	private PdfLoadedField loadedFieldName;

	private Exception exceptionDetails;

	public Exception Exception => exceptionDetails;

	public PdfLoadedField Field => loadedFieldName;

	internal PdfLoadedFieldImportError(PdfLoadedField field, Exception exception)
	{
		loadedFieldName = field;
		exceptionDetails = exception;
	}
}
