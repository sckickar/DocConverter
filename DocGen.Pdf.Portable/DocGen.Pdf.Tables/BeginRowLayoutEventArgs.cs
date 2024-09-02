using System;

namespace DocGen.Pdf.Tables;

public class BeginRowLayoutEventArgs : EventArgs
{
	private int m_rowIndex;

	private PdfCellStyle m_cellStyle;

	private int[] m_spanMap;

	private bool m_bCancel;

	private bool m_bSkip;

	private bool m_ignoreColumnFormat;

	private float m_minHeight;

	private bool m_isRowPaginated;

	internal bool isArgsPropertyModified;

	public int RowIndex => m_rowIndex;

	public PdfCellStyle CellStyle
	{
		get
		{
			return m_cellStyle;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("CellStyle");
			}
			m_cellStyle = value;
			isArgsPropertyModified = true;
		}
	}

	public int[] ColumnSpanMap
	{
		get
		{
			return m_spanMap;
		}
		set
		{
			m_spanMap = value;
			isArgsPropertyModified = true;
		}
	}

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

	public bool IgnoreColumnFormat
	{
		get
		{
			return m_ignoreColumnFormat;
		}
		set
		{
			m_ignoreColumnFormat = value;
		}
	}

	public float MinimalHeight
	{
		get
		{
			return m_minHeight;
		}
		set
		{
			if (value < 0f)
			{
				throw new ArgumentOutOfRangeException("MinimalHeight", "The value can't be less then zero.");
			}
			m_minHeight = value;
		}
	}

	internal bool IsRowPaginated
	{
		get
		{
			return m_isRowPaginated;
		}
		set
		{
			m_isRowPaginated = value;
		}
	}

	internal BeginRowLayoutEventArgs(int rowIndex, PdfCellStyle cellStyle)
	{
		m_rowIndex = rowIndex;
		m_cellStyle = cellStyle;
	}
}
