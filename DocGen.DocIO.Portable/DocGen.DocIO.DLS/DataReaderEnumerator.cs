using System.Collections.Generic;
using System.Data;

namespace DocGen.DocIO.DLS;

internal class DataReaderEnumerator : IRowsEnumerator
{
	private IDataReader m_dataReader;

	private List<List<string>> m_rows;

	private int m_currRowIndex = -1;

	private string[] m_columnNames;

	public int CurrentRowIndex => m_currRowIndex;

	public int RowsCount => m_rows.Count;

	public string TableName => m_dataReader.GetSchemaTable().TableName;

	public bool IsEnd => m_currRowIndex >= RowsCount;

	public bool IsLast => m_currRowIndex >= RowsCount - 1;

	protected List<string> CurrentRow
	{
		get
		{
			if (m_currRowIndex < RowsCount)
			{
				if (m_rows == null)
				{
					return null;
				}
				return m_rows[m_currRowIndex];
			}
			return null;
		}
	}

	public string[] ColumnNames => m_columnNames;

	public DataReaderEnumerator(IDataReader dataReader)
	{
		m_dataReader = dataReader;
		m_rows = new List<List<string>>();
		m_columnNames = new string[m_dataReader.FieldCount];
		for (int i = 0; i < m_dataReader.FieldCount; i++)
		{
			m_columnNames[i] = m_dataReader.GetName(i);
		}
		while (m_dataReader.Read())
		{
			List<string> list = new List<string>();
			for (int j = 0; j < m_dataReader.FieldCount; j++)
			{
				list.Add(m_dataReader[j].ToString());
			}
			m_rows.Add(list);
		}
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
		return CurrentRow?[m_dataReader.GetOrdinal(columnName)];
	}

	internal void Close()
	{
		if (m_dataReader != null)
		{
			m_dataReader.Dispose();
			m_dataReader = null;
		}
		if (m_rows != null)
		{
			for (int i = 0; i < m_rows.Count; i++)
			{
				m_rows[i].Clear();
				m_rows[i] = null;
			}
			m_rows.Clear();
			m_rows = null;
		}
		if (m_columnNames != null)
		{
			m_columnNames = null;
		}
	}
}
