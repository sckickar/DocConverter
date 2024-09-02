using System;
using System.Collections;

namespace DocGen.OfficeChart.Implementation.Collections.Grouping;

internal class MigrantRangeGroup : RangeGroup, IMigrantRange, IRange, IParentApplication, IEnumerable
{
	private IWorksheet sheet;

	public MigrantRangeGroup(IApplication application, object parent)
		: base(application, parent)
	{
		sheet = parent as IWorksheet;
	}

	public void ResetRowColumn(int iRow, int iColumn)
	{
		m_iFirstColumn = (m_iLastColumn = iColumn);
		m_iFirstRow = (m_iLastRow = iRow);
	}

	public void SetValue(int value)
	{
		sheet.SetNumber(m_iFirstRow, m_iFirstColumn, value);
	}

	public void SetValue(double value)
	{
		sheet.SetNumber(m_iFirstRow, m_iFirstColumn, value);
	}

	public void SetValue(DateTime value)
	{
		base.DateTime = value;
	}

	public void SetValue(bool value)
	{
		sheet.SetBoolean(m_iFirstRow, m_iFirstColumn, value);
	}

	public void SetValue(string value)
	{
		sheet.SetText(m_iFirstRow, m_iFirstColumn, value);
	}
}
