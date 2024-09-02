using System;
using System.Collections.Generic;

namespace DocGen.Pdf.Grid;

public class PdfGridRowCollection : List<PdfGridRow>
{
	private PdfGrid m_grid;

	internal PdfGridRowCollection(PdfGrid grid)
	{
		m_grid = grid;
	}

	public PdfGridRow Add()
	{
		PdfGridRow pdfGridRow = new PdfGridRow(m_grid);
		Add(pdfGridRow);
		return pdfGridRow;
	}

    public new void Add(PdfGridRow row)
    {
        PdfGridRowStyle rowStyle = new PdfGridRowStyle();
        if (m_grid.Style != null)
        {
            // Map specific properties here
            rowStyle.BackgroundBrush = m_grid.Style.BackgroundBrush;
            rowStyle.Font = m_grid.Style.Font;
            // Add other property mappings as needed
        }
        row.Style = rowStyle;

        if (row.Cells.Count == 0)
        {
            for (int i = 0; i < m_grid.Columns.Count; i++)
            {
                row.Cells.Add(new PdfGridCell());
            }
        }
        base.Add(row);
    }

    public void SetSpan(int rowIndex, int cellIndex, int rowSpan, int colSpan)
	{
		if (rowIndex > m_grid.Rows.Count)
		{
			throw new IndexOutOfRangeException("rowIndex");
		}
		if (cellIndex > m_grid.Columns.Count)
		{
			throw new IndexOutOfRangeException("cellIndex");
		}
		m_grid.Rows[rowIndex].Cells[cellIndex].RowSpan = rowSpan;
		m_grid.Rows[rowIndex].Cells[cellIndex].ColumnSpan = colSpan;
	}

	public void ApplyStyle(PdfGridStyleBase style)
	{
		if (style is PdfGridCellStyle)
		{
			foreach (PdfGridRow row in m_grid.Rows)
			{
				foreach (PdfGridCell cell in row.Cells)
				{
					cell.Style = style.Clone() as PdfGridCellStyle;
				}
			}
			return;
		}
		if (style is PdfGridRowStyle)
		{
			foreach (PdfGridRow row2 in m_grid.Rows)
			{
				row2.Style = style as PdfGridRowStyle;
			}
			return;
		}
		if (style is PdfGridStyle)
		{
			m_grid.Style = style as PdfGridStyle;
		}
	}
}
