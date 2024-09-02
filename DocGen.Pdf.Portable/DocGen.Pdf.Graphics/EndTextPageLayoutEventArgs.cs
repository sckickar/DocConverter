namespace DocGen.Pdf.Graphics;

public class EndTextPageLayoutEventArgs : EndPageLayoutEventArgs
{
	public new PdfTextLayoutResult Result => base.Result as PdfTextLayoutResult;

	public EndTextPageLayoutEventArgs(PdfTextLayoutResult result)
		: base(result)
	{
	}
}
