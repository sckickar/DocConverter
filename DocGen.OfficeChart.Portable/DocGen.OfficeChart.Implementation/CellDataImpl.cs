using System;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation;

internal class CellDataImpl
{
	private RangeImpl m_range;

	private ICellPositionFormat m_record;

	public RangeImpl Range
	{
		get
		{
			return m_range;
		}
		set
		{
			m_range = value;
		}
	}

	[CLSCompliant(false)]
	public ICellPositionFormat Record
	{
		get
		{
			return m_record;
		}
		set
		{
			m_record = value;
		}
	}
}
