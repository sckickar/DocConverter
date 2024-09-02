using System;
using System.Collections;

namespace DocGen.OfficeChart.Implementation;

internal class MigrantRangeImpl : RangeImpl, IMigrantRange, IRange, IParentApplication, IEnumerable
{
	private IWorksheet sheet;

	public MigrantRangeImpl(IApplication application, IWorksheet parent)
		: base(application, parent)
	{
		sheet = parent;
	}

	public void ResetRowColumn(int iRow, int iColumn)
	{
		m_rtfString = null;
		m_iTopRow = (m_iBottomRow = iRow);
		m_iLeftColumn = (m_iRightColumn = iColumn);
		if (m_style != null)
		{
			m_style.SetFormatIndex(base.ExtendedFormatIndex);
		}
	}

	public void SetValue(int value)
	{
		if (base.NumberFormat.Contains("%"))
		{
			value /= 100;
		}
		sheet.SetNumber(m_iTopRow, m_iLeftColumn, value);
	}

	public void SetValue(double value)
	{
		if (base.NumberFormat.Contains("%"))
		{
			value /= 100.0;
		}
		sheet.SetNumber(m_iTopRow, m_iLeftColumn, value);
	}

	public void SetValue(DateTime value)
	{
		base.DateTime = value;
	}

	public void SetValue(bool value)
	{
		sheet.SetBoolean(m_iTopRow, m_iLeftColumn, value);
	}

	public void SetValue(string value)
	{
		sheet.SetText(m_iTopRow, m_iLeftColumn, value);
	}
}
