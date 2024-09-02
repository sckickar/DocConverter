using System;
using DocGen.Drawing;

namespace DocGen.Pdf.Tables;

public class EndRowLayoutEventArgs : EventArgs
{
	private int m_rowIndex;

	private bool m_bDrawnCompletely;

	private bool m_bCancel;

	private RectangleF m_bounds;

	public int RowIndex => m_rowIndex;

	public bool LayoutCompleted => m_bDrawnCompletely;

	public bool Cancel
	{
		get
		{
			return m_bCancel;
		}
		set
		{
			m_bCancel = value;
		}
	}

	public RectangleF Bounds => m_bounds;

	internal EndRowLayoutEventArgs(int rowIndex, bool drawnCompletely, RectangleF rowBounds)
	{
		m_rowIndex = rowIndex;
		m_bDrawnCompletely = drawnCompletely;
		m_bounds = rowBounds;
	}
}
