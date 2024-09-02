using System.Data;

namespace DocGen.DocIO.DLS;

internal class DataViewEnumerator : IRowsEnumerator
{
	private DataView m_dataView;

	private DataTable m_table;

	private DataRow m_row;

	private int m_currRowIndex = -1;

	private string[] m_columnNames;

	public int CurrentRowIndex => m_currRowIndex;

	public int RowsCount
	{
		get
		{
			if (m_dataView == null)
			{
				return 1;
			}
			return m_dataView.Count;
		}
	}

	public string TableName
	{
		get
		{
			if (m_dataView == null)
			{
				return "";
			}
			return m_dataView.Table.TableName;
		}
	}

	public bool IsEnd => m_currRowIndex >= RowsCount;

	public bool IsLast => m_currRowIndex >= RowsCount - 1;

	protected DataRow CurrentRow
	{
		get
		{
			if (m_currRowIndex < RowsCount)
			{
				if (m_dataView == null)
				{
					return m_row;
				}
				return m_dataView[m_currRowIndex].Row;
			}
			return null;
		}
	}

	public string[] ColumnNames => m_columnNames;

	public DataViewEnumerator(DataView dataView)
	{
		m_dataView = dataView;
		m_table = dataView.Table;
		ReadColumnNames(m_table);
	}

	public void Reset()
	{
		m_currRowIndex = -1;
	}

	public bool NextRow()
	{
		if (m_currRowIndex < RowsCount)
		{
			m_currRowIndex++;
		}
		return !IsEnd;
	}

	public object GetCellValue(string columnName)
	{
		return CurrentRow?[columnName];
	}

	internal void Close()
	{
		if (m_dataView != null)
		{
			m_dataView.Dispose();
			m_dataView = null;
		}
		if (m_table != null)
		{
			m_table.Dispose();
			m_table = null;
		}
		if (m_row != null)
		{
			m_row = null;
		}
		if (m_columnNames != null)
		{
			m_columnNames = null;
		}
	}

	private void ReadColumnNames(DataTable dataTable)
	{
		m_columnNames = new string[dataTable.Columns.Count];
		for (int i = 0; i < m_columnNames.Length; i++)
		{
			m_columnNames[i] = m_table.Columns[i].ColumnName;
		}
	}
}
