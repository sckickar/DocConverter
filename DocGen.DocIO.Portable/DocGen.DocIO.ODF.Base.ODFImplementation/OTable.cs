using System.Collections.Generic;

namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class OTable : OTextBodyItem
{
	private string m_name;

	private string m_styleName;

	private bool m_softPageBreak;

	private List<OTableColumn> m_columns;

	private List<OTableRow> m_rows;

	internal const int MaxColumnCount = 16384;

	internal const int MaxRowCount = 1048576;

	internal string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			m_name = value;
		}
	}

	internal string StyleName
	{
		get
		{
			return m_styleName;
		}
		set
		{
			m_styleName = value;
		}
	}

	internal bool SoftPageBreak
	{
		get
		{
			return m_softPageBreak;
		}
		set
		{
			m_softPageBreak = value;
		}
	}

	internal List<OTableColumn> Columns
	{
		get
		{
			if (m_columns == null)
			{
				m_columns = new List<OTableColumn>();
			}
			return m_columns;
		}
		set
		{
			m_columns = value;
		}
	}

	internal List<OTableRow> Rows
	{
		get
		{
			if (m_rows == null)
			{
				m_rows = new List<OTableRow>();
			}
			return m_rows;
		}
		set
		{
			m_rows = value;
		}
	}

	internal void Dispose()
	{
		if (m_columns != null)
		{
			m_columns.Clear();
			m_columns = null;
		}
		if (m_rows == null)
		{
			return;
		}
		foreach (OTableRow row in m_rows)
		{
			row.Dispose();
		}
		m_rows.Clear();
		m_rows = null;
	}
}
