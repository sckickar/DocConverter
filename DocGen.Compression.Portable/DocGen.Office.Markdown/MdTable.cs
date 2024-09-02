using System.Collections.Generic;

namespace DocGen.Office.Markdown;

internal class MdTable : IMdBlock
{
	private List<MdTableRow> rows;

	private List<ColumnAlignment> m_ColumnAlignments;

	internal List<MdTableRow> Rows
	{
		get
		{
			if (rows == null)
			{
				rows = new List<MdTableRow>();
			}
			return rows;
		}
		set
		{
			rows = value;
		}
	}

	internal List<ColumnAlignment> ColumnAlignments
	{
		get
		{
			if (m_ColumnAlignments == null)
			{
				m_ColumnAlignments = new List<ColumnAlignment>();
			}
			return m_ColumnAlignments;
		}
		set
		{
			m_ColumnAlignments = value;
		}
	}

	internal MdTableRow AddMdTableRow()
	{
		MdTableRow mdTableRow = new MdTableRow();
		Rows.Add(mdTableRow);
		return mdTableRow;
	}

	public void Close()
	{
		foreach (MdTableRow row in Rows)
		{
			row.Close();
		}
		if (rows != null)
		{
			rows.Clear();
			rows = null;
		}
		if (m_ColumnAlignments != null)
		{
			m_ColumnAlignments.Clear();
			m_ColumnAlignments = null;
		}
	}
}
