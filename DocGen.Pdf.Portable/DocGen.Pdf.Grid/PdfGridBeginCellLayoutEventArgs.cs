using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Grid;

public class PdfGridBeginCellLayoutEventArgs : GridCellLayoutEventArgs
{
	private bool m_bSkip;

	private PdfGridCellStyle m_style;

	public bool Skip
	{
		get
		{
			return m_bSkip;
		}
		set
		{
			m_bSkip = value;
		}
	}

	public PdfGridCellStyle Style
	{
		get
		{
			return m_style;
		}
		set
		{
			m_style = value;
		}
	}

	internal PdfGridBeginCellLayoutEventArgs(PdfGraphics graphics, int rowIndex, int cellInder, RectangleF bounds, string value, PdfGridCellStyle style, bool isHeaderRow)
		: base(graphics, rowIndex, cellInder, bounds, value, isHeaderRow)
	{
		m_style = style;
	}
}
