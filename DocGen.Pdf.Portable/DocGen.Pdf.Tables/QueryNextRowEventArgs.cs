using System;

namespace DocGen.Pdf.Tables;

public class QueryNextRowEventArgs : EventArgs
{
	private string[] m_rowData;

	private int m_columnCount;

	private int m_rowIndex;

	public string[] RowData
	{
		get
		{
			return m_rowData;
		}
		set
		{
			if (m_columnCount != 0 && value != null && value.Length != m_columnCount)
			{
				throw new ArgumentException("The data array is not of the proper length.", "RowData");
			}
			m_rowData = value;
		}
	}

	public int ColumnCount => m_columnCount;

	public int RowIndex => m_rowIndex;

	internal QueryNextRowEventArgs(int columnCount, int rowIndex)
	{
		if (columnCount < 0)
		{
			throw new ArgumentOutOfRangeException("columnCount");
		}
		m_columnCount = columnCount;
		m_rowIndex = rowIndex;
	}
}
