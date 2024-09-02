using System.Collections.Generic;

namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class OTableRow : OTableColumn
{
	private List<OTableCell> m_cells;

	internal List<OTableCell> Cells
	{
		get
		{
			if (m_cells == null)
			{
				m_cells = new List<OTableCell>();
			}
			return m_cells;
		}
		set
		{
			m_cells = value;
		}
	}

	internal void Dispose()
	{
		if (m_cells == null)
		{
			return;
		}
		foreach (OTableCell cell in m_cells)
		{
			cell.Dispose();
		}
		m_cells.Clear();
		m_cells = null;
	}
}
