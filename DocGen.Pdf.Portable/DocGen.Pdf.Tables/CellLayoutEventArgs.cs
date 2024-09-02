using System;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Tables;

public abstract class CellLayoutEventArgs : EventArgs
{
	private int m_rowIndex;

	private int m_cellIndex;

	private string m_value;

	private RectangleF m_bounds;

	private PdfGraphics m_graphics;

	public int RowIndex => m_rowIndex;

	public int CellIndex => m_cellIndex;

	public string Value => m_value;

	public RectangleF Bounds => m_bounds;

	public PdfGraphics Graphics => m_graphics;

	internal CellLayoutEventArgs(PdfGraphics graphics, int rowIndex, int cellInder, RectangleF bounds, string value)
	{
		if (graphics == null)
		{
			throw new ArgumentNullException("graphics");
		}
		m_rowIndex = rowIndex;
		m_cellIndex = cellInder;
		m_value = value;
		m_bounds = bounds;
		m_graphics = graphics;
	}
}
