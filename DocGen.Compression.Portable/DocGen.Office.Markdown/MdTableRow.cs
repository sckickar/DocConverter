using System.Collections.Generic;

namespace DocGen.Office.Markdown;

internal class MdTableRow
{
	private List<MdTableCell> m_cells;

	internal List<MdTableCell> Cells
	{
		get
		{
			if (m_cells == null)
			{
				m_cells = new List<MdTableCell>();
			}
			return m_cells;
		}
		set
		{
			m_cells = value;
		}
	}

	internal MdTableCell AddMdTableCell()
	{
		MdTableCell mdTableCell = new MdTableCell();
		Cells.Add(mdTableCell);
		return mdTableCell;
	}

	internal void Close()
	{
		foreach (MdTableCell cell in m_cells)
		{
			cell.Close();
		}
		if (m_cells != null)
		{
			m_cells.Clear();
			m_cells = null;
		}
	}
}
