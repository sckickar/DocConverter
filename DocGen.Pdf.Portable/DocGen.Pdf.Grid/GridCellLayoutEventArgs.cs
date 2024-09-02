using System;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Grid;

public abstract class GridCellLayoutEventArgs : EventArgs
{
	private int m_rowIndex;

	private int m_cellIndex;

	private string m_value;

	private RectangleF m_bounds;

	private PdfGraphics m_graphics;

	private bool m_isHeaderRow;

	public int RowIndex => m_rowIndex;

	public int CellIndex => m_cellIndex;

	public string Value => m_value;

	public RectangleF Bounds => m_bounds;

	public PdfGraphics Graphics => m_graphics;

	public bool IsHeaderRow => m_isHeaderRow;

	internal GridCellLayoutEventArgs(PdfGraphics graphics, int rowIndex, int cellIndex, RectangleF bounds, string value, bool isHeaderRow)
	{
		if (graphics == null)
		{
			throw new ArgumentNullException("graphics");
		}
		m_rowIndex = rowIndex;
		m_cellIndex = cellIndex;
		m_value = value;
		m_bounds = bounds;
		m_graphics = graphics;
		m_isHeaderRow = isHeaderRow;
	}
}
