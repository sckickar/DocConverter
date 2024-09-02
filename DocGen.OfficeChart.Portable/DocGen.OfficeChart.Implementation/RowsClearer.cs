using System;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Interfaces;

namespace DocGen.OfficeChart.Implementation;

internal class RowsClearer : IOperation
{
	private WorksheetImpl m_sheet;

	private int m_iIndex;

	private int m_iCount;

	public RowsClearer(WorksheetImpl sheet, int index, int count)
	{
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		m_sheet = sheet;
		m_iIndex = index;
		m_iCount = count;
	}

	public void Do()
	{
		ArrayListEx rows = m_sheet.CellRecords.Table.Rows;
		int num = 0;
		int num2 = m_iIndex - 1;
		while (num < m_iCount)
		{
			RowStorage rowStorage = rows[num2];
			rows[num2] = null;
			rowStorage?.Dispose();
			num++;
			num2++;
		}
	}
}
