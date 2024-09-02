using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace DocGen.Pdf.Tables;

internal class PdfDataSource
{
	private DataTable m_dataTable;

	private int m_rowCount;

	private int m_colCount;

	private DataColumn m_dataColumn;

	private Array m_array;

	private bool m_useSorting = true;

	private DataRow[] m_cachRows;

	internal bool UseSorting
	{
		get
		{
			return m_useSorting;
		}
		set
		{
			m_useSorting = value;
		}
	}

	public int RowCount => m_rowCount;

	public int ColumnCount => GetVisibleColCount();

	public string[] ColumnNames => GetColumnsNames();

	public string[] ColumnCaptions => GetColumnsCaptions();

	private PdfDataSource()
	{
	}

	public PdfDataSource(DataTable table)
	{
		if (table == null)
		{
			throw new ArgumentNullException("Data table can't be null", "table");
		}
		SetTable(table);
	}

	internal PdfDataSource(IEnumerable customSource)
	{
		if (customSource == null)
		{
			return;
		}
		DataTable dataTable = new DataTable();
		PropertyInfo[] array = null;
		PdfRow pdfRow = new PdfRow();
		List<string> list = new List<string>();
		foreach (object item in customSource)
		{
			if (item != null)
			{
				array = item.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
				PropertyInfo[] array2 = array;
				foreach (PropertyInfo propertyInfo in array2)
				{
					dataTable.Columns.Add(propertyInfo.Name);
					list.Add(propertyInfo.Name);
				}
				break;
			}
		}
		object[] values = list.ToArray();
		pdfRow.Values = values;
		dataTable.Rows.Add(pdfRow.Values);
		_ = dataTable.Columns.Count;
		foreach (object item2 in customSource)
		{
			list = new List<string>();
			PropertyInfo[] array2 = array;
			foreach (PropertyInfo propertyInfo2 in array2)
			{
				PropertyInfo property = item2.GetType().GetProperty(propertyInfo2.Name);
				list.Add(Convert.ToString(property.GetValue(item2, null)));
			}
			values = list.ToArray();
			pdfRow.Values = values;
			dataTable.Rows.Add(pdfRow.Values);
		}
		if (dataTable == null)
		{
			throw new ArgumentNullException("Data table can't be null", "table");
		}
		SetTable(dataTable);
	}

	public PdfDataSource(DataSet dataSet, string tableName)
		: this(GetTableFromDataSet(dataSet, tableName))
	{
	}

	public PdfDataSource(DataView view)
		: this(GetTableFromDataView(view))
	{
	}

	public PdfDataSource(DataColumn column)
	{
		if (column == null)
		{
			throw new ArgumentNullException("Column can't be null", "column");
		}
		if (column.Table == null)
		{
			throw new ArgumentNullException("Data column must belong to some table", "column");
		}
		m_dataColumn = column;
		m_colCount = 1;
		m_rowCount = m_dataColumn.Table.Rows.Count;
	}

	public PdfDataSource(Array array)
	{
		if (array == null)
		{
			throw new ArgumentException("Array can'n be null", "array");
		}
		if (!IsArrayValid(array, ref m_colCount))
		{
			throw new ArgumentException("We don't suuport more than one or two dimensions arrays in this context or you array has diiferent length", "array");
		}
		m_array = array;
		m_rowCount = m_array.GetLength(0);
	}

	public string[] GetRow(ref int index)
	{
		if (index < 0)
		{
			throw new IndexOutOfRangeException("The index must be less than rows count ormore or equels than zero");
		}
		string[] result = null;
		if (index < m_rowCount)
		{
			if (m_dataTable != null)
			{
				result = GetRowFromTable(m_dataTable, ref index);
			}
			if (m_dataColumn != null)
			{
				result = GetRowFromColumn(m_dataColumn, ref index);
			}
			if (m_array != null)
			{
				result = GetRowFromArray(m_array, ref index);
			}
		}
		return result;
	}

	public bool IsColumnReadOnly(int index)
	{
		if (index < 0 || index >= GetVisibleColCount())
		{
			throw new IndexOutOfRangeException("The index must be less than columns count ormore than or equal to zero.");
		}
		bool result = false;
		if (m_dataTable != null)
		{
			int visibleIndex = GetVisibleIndex(index);
			result = m_dataTable.Columns[visibleIndex].ReadOnly;
		}
		if (m_dataColumn != null)
		{
			result = m_dataColumn.ReadOnly;
		}
		if (m_array != null)
		{
			result = m_array.IsReadOnly;
		}
		return result;
	}

	public MappingType GetColumnMappingType(int index)
	{
		if (index < 0 || index >= GetVisibleColCount())
		{
			throw new IndexOutOfRangeException("The index must be less than columns count ormore or equels than zero");
		}
		MappingType result = MappingType.Hidden;
		if (m_dataTable != null)
		{
			int visibleIndex = GetVisibleIndex(index);
			result = m_dataTable.Columns[visibleIndex].ColumnMapping;
		}
		if (m_dataColumn != null)
		{
			result = m_dataColumn.ColumnMapping;
		}
		if (m_array != null)
		{
			throw new ArgumentException("Array does not have mapping type propety");
		}
		return result;
	}

	public Type GetColumnDataType(int index)
	{
		if (index < 0 || index >= GetVisibleColCount())
		{
			throw new IndexOutOfRangeException("The index must be less than columns count ormore or equels than zero");
		}
		Type result = null;
		if (m_dataTable != null)
		{
			int visibleIndex = GetVisibleIndex(index);
			result = m_dataTable.Columns[visibleIndex].DataType;
		}
		if (m_dataColumn != null)
		{
			result = m_dataColumn.DataType;
		}
		if (m_array != null)
		{
			result = GetTypeOfArray(m_array);
		}
		return result;
	}

	public object GetColumnDefaultValue(int index)
	{
		if (index < 0 || index >= GetVisibleColCount())
		{
			throw new IndexOutOfRangeException("The index must be less than columns count ormore or equels than zero");
		}
		object result = null;
		if (m_dataTable != null)
		{
			int visibleIndex = GetVisibleIndex(index);
			result = m_dataTable.Columns[visibleIndex].DefaultValue;
		}
		if (m_dataColumn != null)
		{
			result = m_dataColumn.DefaultValue;
		}
		if (m_array != null)
		{
			throw new ArgumentException("Array does not have default value propety");
		}
		return result;
	}

	public bool AllowDBNull(int index)
	{
		if (index < 0 || index >= GetVisibleColCount())
		{
			throw new IndexOutOfRangeException("The index must be less than columns count ormore or equels than zero");
		}
		bool result = false;
		if (m_dataTable != null)
		{
			int visibleIndex = GetVisibleIndex(index);
			result = m_dataTable.Columns[visibleIndex].AllowDBNull;
		}
		if (m_dataColumn != null)
		{
			result = m_dataColumn.AllowDBNull;
		}
		if (m_array != null)
		{
			throw new ArgumentException("Array does not have allowDBNull propety");
		}
		return result;
	}

	private Type GetTypeOfArray(Array array)
	{
		Type result = null;
		switch (array.Rank)
		{
		case 1:
			result = ((!(array.GetValue(0) is Array array2)) ? array.GetValue(0).GetType() : array2.GetValue(0).GetType());
			break;
		case 2:
			result = array.GetValue(0, 0).GetType();
			break;
		}
		return result;
	}

	private string[] GetColumnsNames()
	{
		string[] result = null;
		if (m_dataTable != null)
		{
			List<string> list = new List<string>();
			for (int i = 0; i < m_colCount; i++)
			{
				DataColumn dataColumn = m_dataTable.Columns[i];
				if (dataColumn.ColumnMapping != MappingType.Hidden)
				{
					list.Add(dataColumn.ColumnName);
				}
			}
			result = list.ToArray();
		}
		if (m_dataColumn != null && m_dataColumn.ColumnMapping != MappingType.Hidden)
		{
			result = new string[1] { m_dataColumn.ColumnName };
		}
		return result;
	}

	private string[] GetColumnsCaptions()
	{
		string[] result = null;
		if (m_dataTable != null)
		{
			List<string> list = new List<string>();
			for (int i = 0; i < m_colCount; i++)
			{
				DataColumn dataColumn = m_dataTable.Columns[i];
				if (dataColumn.ColumnMapping != MappingType.Hidden)
				{
					if (dataColumn.Caption != string.Empty)
					{
						list.Add(dataColumn.Caption);
					}
					else
					{
						list.Add(dataColumn.ColumnName);
					}
				}
			}
			result = list.ToArray();
		}
		if (m_dataColumn != null && m_dataColumn.ColumnMapping != MappingType.Hidden)
		{
			result = ((!(m_dataColumn.Caption != string.Empty)) ? new string[1] { m_dataColumn.ColumnName } : new string[1] { m_dataColumn.Caption });
		}
		return result;
	}

	private bool IsArrayValid(Array array, ref int count)
	{
		bool result = false;
		switch (array.Rank)
		{
		case 1:
		{
			int num = 0;
			if (array.GetValue(0) is Array array2)
			{
				if (array2.Rank > 1)
				{
					result = false;
					break;
				}
				num = array2.GetLength(0);
				int i = 1;
				for (int length = array.Length; i < length; i++)
				{
					int num2 = 0;
					if (array.GetValue(i) is Array array3)
					{
						if (array3.Rank > 1)
						{
							result = false;
							break;
						}
						num2 = array3.GetLength(0);
					}
					if (num != num2)
					{
						result = false;
						break;
					}
					result = true;
					count = num;
				}
			}
			else
			{
				num = (count = num + 1);
				result = true;
			}
			break;
		}
		case 2:
			count = array.GetLength(1);
			result = true;
			break;
		default:
			result = false;
			break;
		}
		return result;
	}

	private void SetTable(DataTable table)
	{
		if (table.Columns.Count == 0)
		{
			table.Columns.Add("Col0");
		}
		m_dataTable = table;
		m_colCount = m_dataTable.Columns.Count;
		m_rowCount = m_dataTable.Rows.Count;
		m_dataTable.ColumnChanged += dataTable_ColumnChanged;
		m_dataTable.RowChanged += dataTable_RowChanged;
		m_dataTable.RowDeleted += dataTable_RowDeleted;
	}

	private void dataTable_RowDeleted(object sender, DataRowChangeEventArgs e)
	{
		RefreshCache();
	}

	private void dataTable_RowChanged(object sender, DataRowChangeEventArgs e)
	{
		RefreshCache();
	}

	private void dataTable_ColumnChanged(object sender, DataColumnChangeEventArgs e)
	{
		RefreshCache();
	}

	private void RefreshCache()
	{
		m_cachRows = null;
	}

	private int GetVisibleColCount()
	{
		int num = 0;
		if (m_dataTable != null)
		{
			for (int i = 0; i < m_colCount; i++)
			{
				if (m_dataTable.Columns[i].ColumnMapping != MappingType.Hidden)
				{
					num++;
				}
			}
		}
		if (m_dataColumn != null)
		{
			num = ((m_dataColumn.ColumnMapping != MappingType.Hidden) ? 1 : 0);
		}
		if (m_array != null)
		{
			num = m_colCount;
		}
		return num;
	}

	private int GetVisibleIndex(int index)
	{
		if (index < 0 || index >= GetVisibleColCount())
		{
			throw new IndexOutOfRangeException("The index must be less than columns count ormore than or equel to zero");
		}
		int result = 0;
		if (m_dataTable != null)
		{
			int num = index;
			int num2 = 0;
			while (num > -1)
			{
				if (m_dataTable.Columns[num2].ColumnMapping == MappingType.Hidden)
				{
					num2++;
					continue;
				}
				num--;
				num2++;
			}
			result = num2 - 1;
		}
		if (m_dataColumn != null)
		{
			if (m_dataColumn.ColumnMapping == MappingType.Hidden)
			{
				throw new ArgumentException("The source is DataColumn, but this column is hidden");
			}
			result = 0;
		}
		if (m_array != null)
		{
			result = index;
		}
		return result;
	}

	private string[] GetRowFromArray(Array array, ref int index)
	{
		string[] array2 = null;
		switch (array.Rank)
		{
		case 1:
		{
			if (!(array.GetValue(0) is Array))
			{
				array2 = new string[m_colCount];
				for (int j = 0; j < m_colCount; j++)
				{
					object value2 = array.GetValue(index);
					array2[j] = Convert.ToString(value2);
				}
				break;
			}
			array2 = new string[m_colCount];
			Array array4 = array.GetValue(index) as Array;
			for (int k = 0; k < m_colCount; k++)
			{
				object value3 = array4.GetValue(k);
				array2[k] = Convert.ToString(value3);
			}
			break;
		}
		case 2:
		{
			array2 = new string[m_colCount];
			for (int i = 0; i < m_colCount; i++)
			{
				object value = array.GetValue(index, i);
				array2[i] = Convert.ToString(value);
			}
			break;
		}
		default:
			throw new ArgumentException("We don't suuport more than one or two dimensions arrays in this context or you array has diiferent length", "array");
		}
		index++;
		return array2;
	}

	private string[] GetRowFromColumn(DataColumn dataColumn, ref int index)
	{
		if (dataColumn.ColumnMapping == MappingType.Hidden)
		{
			throw new ArgumentException("The source is DataColumn, but this column is hidden");
		}
		string[] array = null;
		if (m_useSorting)
		{
			if (m_cachRows == null)
			{
				m_cachRows = dataColumn.Table.Select();
			}
			object obj = m_cachRows[index][dataColumn.ColumnName];
			array = new string[1] { Convert.ToString(obj) };
		}
		else
		{
			object obj2 = dataColumn.Table.Rows[index][dataColumn.ColumnName];
			array = new string[1] { Convert.ToString(obj2) };
		}
		index++;
		return array;
	}

	private string[] GetRowFromTable(DataTable dataTable, ref int index)
	{
		if (dataTable.Rows.Count <= 0)
		{
			throw new ArgumentException("There is no rows in data source");
		}
		if (index < 0 || index >= dataTable.Rows.Count)
		{
			throw new IndexOutOfRangeException("The index must be less than rows count ormore or equels than zero");
		}
		object[] array = null;
		if (m_useSorting)
		{
			if (m_cachRows == null)
			{
				m_cachRows = dataTable.Select();
			}
			array = m_cachRows[index].ItemArray;
		}
		else
		{
			array = dataTable.Rows[index].ItemArray;
		}
		List<string> list = new List<string>();
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			if (dataTable.Columns[i].ColumnMapping != MappingType.Hidden)
			{
				list.Add(Convert.ToString(array[i]));
			}
		}
		index++;
		return list.ToArray();
	}

	private static DataTable GetTableFromDataSet(DataSet dataSet, string tableName)
	{
		if (dataSet == null)
		{
			throw new ArgumentNullException("Data Set can't be null", "dataSet");
		}
		if (dataSet.Tables.Count <= 0)
		{
			throw new ArgumentException("The data set should contain at least one data table", "dataSet");
		}
		DataTable dataTable = null;
		if (tableName != null && tableName != string.Empty)
		{
			if (!dataSet.Tables.Contains(tableName))
			{
				throw new ArgumentNullException("The data set should contain a table with specified table name", tableName);
			}
			return dataSet.Tables[tableName];
		}
		return dataSet.Tables[0];
	}

	private static DataTable GetTableFromDataView(DataView view)
	{
		if (view == null)
		{
			throw new ArgumentNullException("Data view", "view");
		}
		return view.Table;
	}
}
