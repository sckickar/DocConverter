using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Grid;

public class PdfGridEndPageLayoutEventArgs : EndPageLayoutEventArgs
{
	internal PdfGridEndPageLayoutEventArgs(PdfLayoutResult result)
		: base(result)
	{
	}
}
