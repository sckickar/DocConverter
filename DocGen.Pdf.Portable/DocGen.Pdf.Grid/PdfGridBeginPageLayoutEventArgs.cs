using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Grid;

public class PdfGridBeginPageLayoutEventArgs : BeginPageLayoutEventArgs
{
	private int m_startRow;

	public int StartRowIndex => m_startRow;

	internal PdfGridBeginPageLayoutEventArgs(RectangleF bounds, PdfPage page, int startRow)
		: base(bounds, page)
	{
		m_startRow = startRow;
	}
}
