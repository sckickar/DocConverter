using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Grid;

public class PdfGridEndCellLayoutEventArgs : GridCellLayoutEventArgs
{
	private PdfGridCellStyle m_style;

	public PdfGridCellStyle Style => m_style;

	internal PdfGridEndCellLayoutEventArgs(PdfGraphics graphics, int rowIndex, int cellInder, RectangleF bounds, string value, PdfGridCellStyle style, bool isHeaderRow)
		: base(graphics, rowIndex, cellInder, bounds, value, isHeaderRow)
	{
		m_style = style;
	}
}
