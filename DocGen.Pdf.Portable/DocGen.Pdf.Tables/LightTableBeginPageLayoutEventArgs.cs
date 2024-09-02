using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Tables;

public class LightTableBeginPageLayoutEventArgs : BeginPageLayoutEventArgs
{
	private int m_startRow;

	public int StartRowIndex => m_startRow;

	internal LightTableBeginPageLayoutEventArgs(RectangleF bounds, PdfPage page, int startRow)
		: base(bounds, page)
	{
		m_startRow = startRow;
	}
}
