namespace DocGen.Pdf.Parsing;

public class PdfRevision
{
	private long startPosition;

	public long StartPosition
	{
		get
		{
			return startPosition;
		}
		internal set
		{
			startPosition = value;
		}
	}
}
