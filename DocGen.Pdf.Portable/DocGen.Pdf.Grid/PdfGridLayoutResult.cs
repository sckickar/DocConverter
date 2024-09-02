using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Grid;

public class PdfGridLayoutResult : PdfLayoutResult
{
	public PdfGridLayoutResult(PdfPage page, RectangleF bounds)
		: base(page, bounds)
	{
	}
}
