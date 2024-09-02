using System;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Grid;

public class PdfGridRow
{
	private PdfGridCellCollection m_cells;

	private PdfGrid m_grid;

	private PdfGridRowStyle m_style;

	private float m_height = float.MinValue;

	private float m_width = float.MinValue;

	private bool m_bRowSpanExists;

	private bool m_bColumnSpanExists;

	private float m_rowBreakHeight;

	private int m_rowOverflowIndex;

	private PdfLayoutResult m_gridResult;

	internal bool isRowBreaksNextPage;

	internal float rowBreakHeight;

	internal bool isrowFinish;

	internal bool isComplete;

	private bool m_rowMergeComplete = true;

	internal int m_noOfPageCount;

	internal bool m_isRowHeightSet;

	private PdfTag m_tag;

	internal int maximumRowSpan;

	internal bool isPageBreakRowSpanApplied;

	internal bool m_isRowSpanRowHeightSet;

	internal float m_rowSpanRemainingHeight;

	internal bool m_isHeaderRow;

	internal bool m_drawCellBroders;

	internal float m_borderReminingHeight;

	internal bool m_paginatedGridRow;

	public PdfGridCellCollection Cells
	{
		get
		{
			if (m_cells == null)
			{
				m_cells = new PdfGridCellCollection(this);
			}
			return m_cells;
		}
	}

	internal PdfGrid Grid
	{
		get
		{
			return m_grid;
		}
		set
		{
			m_grid = value;
		}
	}

	internal bool IsHeaderRow
	{
		get
		{
			return m_isHeaderRow;
		}
		set
		{
			m_isHeaderRow = value;
		}
	}

	public PdfGridRowStyle Style
	{
		get
		{
			if (m_style == null)
			{
				m_style = new PdfGridRowStyle();
			}
			return m_style;
		}
		set
		{
			m_style = value;
		}
	}

	public float Height
	{
		get
		{
			if (!m_isRowHeightSet)
			{
				m_height = MeasureHeight();
			}
			return m_height;
		}
		set
		{
			m_height = value;
			m_isRowHeightSet = true;
		}
	}

	internal float Width
	{
		get
		{
			if (m_width == float.MinValue)
			{
				m_width = MeasureWidth();
			}
			return m_width;
		}
	}

	internal bool RowSpanExists
	{
		get
		{
			return m_bRowSpanExists;
		}
		set
		{
			m_bRowSpanExists = value;
		}
	}

	internal bool ColumnSpanExists
	{
		get
		{
			return m_bColumnSpanExists;
		}
		set
		{
			m_bColumnSpanExists = value;
		}
	}

	internal float RowBreakHeight
	{
		get
		{
			return m_rowBreakHeight;
		}
		set
		{
			m_rowBreakHeight = value;
		}
	}

	internal int RowOverflowIndex
	{
		get
		{
			return m_rowOverflowIndex;
		}
		set
		{
			m_rowOverflowIndex = value;
		}
	}

	internal PdfLayoutResult NestedGridLayoutResult
	{
		get
		{
			return m_gridResult;
		}
		set
		{
			m_gridResult = value;
		}
	}

	internal int RowIndex => Grid.Rows.IndexOf(this);

	internal bool RowMergeComplete
	{
		get
		{
			return m_rowMergeComplete;
		}
		set
		{
			m_rowMergeComplete = value;
		}
	}

	public PdfTag PdfTag
	{
		get
		{
			return m_tag;
		}
		set
		{
			m_tag = value;
		}
	}

	public PdfGridRow(PdfGrid grid)
	{
		m_grid = grid;
	}

	private float MeasureHeight()
	{
		float num = 0f;
		bool flag = false;
		float num2 = 0f;
		float num3 = ((Cells[0].RowSpan <= 1) ? Cells[0].Height : 0f);
		if (Grid.Headers.IndexOf(this) != -1)
		{
			flag = true;
		}
		foreach (PdfGridCell cell in Cells)
		{
			if (cell.m_rowSpanRemainingHeight > num)
			{
				num = cell.m_rowSpanRemainingHeight;
			}
			if (cell.IsRowMergeContinue)
			{
				continue;
			}
			if (!cell.IsRowMergeContinue)
			{
				RowMergeComplete = false;
			}
			if (cell.RowSpan > 1)
			{
				if (num2 < cell.Height)
				{
					num2 = cell.Height;
				}
			}
			else
			{
				num3 = Math.Max(num3, cell.Height);
			}
		}
		if (num3 == 0f)
		{
			num3 = num2;
		}
		else if (num > 0f)
		{
			num3 += num;
		}
		if (flag && num2 != 0f && num3 != 0f && num3 < num2)
		{
			num3 = num2;
		}
		return num3;
	}

	private float MeasureWidth()
	{
		float num = 0f;
		foreach (PdfGridColumn column in Grid.Columns)
		{
			num += column.Width;
		}
		return num;
	}

	public void ApplyStyle(PdfGridCellStyle cellStyle)
	{
		foreach (PdfGridCell cell in Cells)
		{
			cell.Style = cellStyle;
		}
	}

	internal PdfGridRow CloneGridRow()
	{
		return (PdfGridRow)MemberwiseClone();
	}
}
