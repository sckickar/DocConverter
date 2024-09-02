using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Tables;

public class BeginCellLayoutEventArgs : CellLayoutEventArgs
{
	private bool m_bSkip;

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

	internal BeginCellLayoutEventArgs(PdfGraphics graphics, int rowIndex, int cellInder, RectangleF bounds, string value)
		: base(graphics, rowIndex, cellInder, bounds, value)
	{
	}
}
