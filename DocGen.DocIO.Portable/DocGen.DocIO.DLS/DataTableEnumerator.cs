using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace DocGen.DocIO.DLS;

internal class DataTableEnumerator : IRowsEnumerator
{
	private DataTable m_table;

	private DataRow m_row;

	private int m_currRowIndex = -1;

	private string[] m_columnsNames;

	private int m_matchingRecordsCount;

	private string m_command;

	private string m_tableName;

	internal IEnumerator m_MMtable;

	private Type m_userClassType;

	private int m_rowCount;

	public int CurrentRowIndex => m_currRowIndex;

	public int RowsCount
	{
		get
		{
			if (m_table != null)
			{
				return m_table.Rows.Count;
			}
			if (m_rowCount == 0 && m_row != null && m_row.ItemArray != null && m_row.ItemArray.Length != 0)
			{
				return 1;
			}
			return m_rowCount;
		}
	}

	public string TableName
	{
		get
		{
			if (m_table != null)
			{
				return m_table.TableName;
			}
			return m_tableName;
		}
	}

	public bool IsEnd => m_currRowIndex >= RowsCount;

	public bool IsLast => m_currRowIndex >= RowsCount - 1;

	protected object CurrentRow
	{
		get
		{
			if (m_currRowIndex < RowsCount)
			{
				if (m_MMtable != null)
				{
					m_MMtable.Reset();
					for (int i = 0; i <= m_currRowIndex; i++)
					{
						m_MMtable.MoveNext();
					}
					return m_MMtable.Current;
				}
				if (m_table != null)
				{
					return m_table.Rows[m_currRowIndex];
				}
				return m_row;
			}
			return null;
		}
	}

	internal int MatchingRecordsCount
	{
		get
		{
			return m_matchingRecordsCount;
		}
		set
		{
			m_matchingRecordsCount = value;
		}
	}

	internal string Command
	{
		get
		{
			return m_command;
		}
		set
		{
			m_command = value;
		}
	}

	public string[] ColumnNames => m_columnsNames;

	public DataTableEnumerator(DataTable table)
	{
		m_table = table;
		ReadColumnNames(m_table);
	}

	public DataTableEnumerator(DataRow row)
	{
		m_row = row;
		ReadColumnNames(row.Table);
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

	internal bool NextRow(string[] command)
	{
		if (m_currRowIndex < RowsCount)
		{
			m_currRowIndex++;
			if (command.Length > 2)
			{
				while (m_currRowIndex < RowsCount && CurrentRow != null)
				{
					CurrentRow.GetType();
					if (CurrentRow is IDictionary<string, object>)
					{
						if ((CurrentRow as IDictionary<string, object>).ContainsKey(command[0]) && ((CurrentRow as IDictionary<string, object>)[command[0]].ToString() == command[2] || (command[1].ToLower() == "contains" && (CurrentRow as IDictionary<string, object>)[command[0]].ToString().Contains(command[2]))))
						{
							break;
						}
						if (m_currRowIndex == RowsCount - 1)
						{
							m_currRowIndex++;
							return true;
						}
						m_currRowIndex++;
						continue;
					}
					PropertyInfo[] properties = CurrentRow.GetType().GetProperties();
					int i = 0;
					for (int num = properties.Length; i < num; i++)
					{
						PropertyInfo propertyInfo = properties[i];
						if (propertyInfo.Name == command[0] && propertyInfo.GetValue(CurrentRow, null).ToString() == command[2])
						{
							return !IsEnd;
						}
					}
					if (m_currRowIndex == RowsCount - 1)
					{
						m_currRowIndex++;
						return true;
					}
					m_currRowIndex++;
				}
			}
		}
		return !IsEnd;
	}

	public object GetCellValue(string columnName)
	{
		if (CurrentRow is DataRow)
		{
			return (CurrentRow as DataRow)[columnName];
		}
		if (CurrentRow != null)
		{
			CurrentRow.GetType();
			if (CurrentRow is IDictionary<string, object>)
			{
				if ((CurrentRow as IDictionary<string, object>).ContainsKey(columnName))
				{
					return (CurrentRow as IDictionary<string, object>)[columnName];
				}
				return string.Empty;
			}
			PropertyInfo[] properties = m_userClassType.GetProperties();
			int i = 0;
			for (int num = properties.Length; i < num; i++)
			{
				PropertyInfo propertyInfo = properties[i];
				if (propertyInfo.Name == columnName)
				{
					return propertyInfo.GetValue(CurrentRow, null);
				}
			}
		}
		return null;
	}

	internal void Close()
	{
		if (m_table != null)
		{
			m_table.Dispose();
			m_table = null;
		}
		if (m_row != null)
		{
			m_row = null;
		}
		if (m_columnsNames != null)
		{
			m_columnsNames = null;
		}
	}

	private void ReadColumnNames(DataTable table)
	{
		m_columnsNames = new string[table.Columns.Count];
		for (int i = 0; i < m_columnsNames.Length; i++)
		{
			m_columnsNames[i] = table.Columns[i].ColumnName;
		}
	}

	public DataTableEnumerator(MailMergeDataTable table)
	{
		m_tableName = table.GroupName;
		table.SourceData.Reset();
		table.SourceData.MoveNext();
		MatchingRecordsCount = table.MatchingRecordsCount;
		Command = table.Command;
		m_MMtable = table.SourceData;
		try
		{
			m_userClassType = m_MMtable.Current.GetType();
			if (m_MMtable.Current is IDictionary<string, object>)
			{
				ReadColumnNames(table);
			}
			else if (m_MMtable.Current is DataRow)
			{
				ReadColumnNamesInTable(m_MMtable);
			}
			else
			{
				ReadColumnNames(m_MMtable);
			}
		}
		catch
		{
			m_userClassType = null;
			m_columnsNames = null;
		}
		CalculRowCount();
	}

	private void ReadColumnNames(MailMergeDataTable table)
	{
		List<string> list = new List<string>();
		table.SourceData.Reset();
		while (table.SourceData.MoveNext())
		{
			foreach (KeyValuePair<string, object> item in table.SourceData.Current as IDictionary<string, object>)
			{
				if (!list.Contains(item.Key))
				{
					list.Add(item.Key);
				}
			}
		}
		m_columnsNames = list.ToArray();
	}

	private void ReadColumnNamesInTable(IEnumerator table)
	{
		List<string> list = new List<string>();
		if (table.Current is DataRow dataRow)
		{
			DataTable table2 = dataRow.Table;
			for (int i = 0; i < table2.Columns.Count; i++)
			{
				list.Add(table2.Columns[i].ColumnName);
			}
		}
		m_columnsNames = list.ToArray();
	}

	private void ReadColumnNames(IEnumerator table)
	{
		List<string> list = new List<string>();
		PropertyInfo[] properties = m_userClassType.GetProperties();
		int i = 0;
		for (int num = properties.Length; i < num; i++)
		{
			list.Add(properties[i].Name);
		}
		m_columnsNames = list.ToArray();
	}

	private void CalculRowCount()
	{
		m_MMtable.Reset();
		while (m_MMtable.MoveNext())
		{
			m_rowCount++;
		}
		m_MMtable.Reset();
	}
}
