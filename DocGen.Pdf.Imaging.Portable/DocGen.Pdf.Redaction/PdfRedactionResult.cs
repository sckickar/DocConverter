using DocGen.Drawing;

namespace DocGen.Pdf.Redaction;

public class PdfRedactionResult
{
	private RectangleF redactionBounds;

	private bool isRedactionSuccess;

	private int pageNumber;

	public RectangleF RedactionBounds => redactionBounds;

	public bool IsRedactionSuccess => isRedactionSuccess;

	public int PageNumber => pageNumber;

	internal PdfRedactionResult(int pageNumber, bool success, RectangleF bounds)
	{
		this.pageNumber = pageNumber;
		redactionBounds = bounds;
		isRedactionSuccess = success;
	}
}
