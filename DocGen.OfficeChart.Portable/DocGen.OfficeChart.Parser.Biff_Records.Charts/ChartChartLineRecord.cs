using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartChartLine)]
[CLSCompliant(false)]
internal class ChartChartLineRecord : BiffRecordRaw
{
	private const int DefaultRecordSize = 2;

	[BiffRecordPos(0, 2)]
	private ushort m_usDropLines;

	private bool m_hasDropLine;

	private bool m_hasHighLowLine;

	private bool m_hasSeriesLine;

	public ExcelDropLineStyle LineStyle
	{
		get
		{
			return (ExcelDropLineStyle)m_usDropLines;
		}
		set
		{
			m_usDropLines = (ushort)value;
			if (m_usDropLines == 0)
			{
				m_hasDropLine = true;
			}
			else if (m_usDropLines == 1)
			{
				m_hasHighLowLine = true;
			}
			else if (m_usDropLines == 2)
			{
				m_hasSeriesLine = true;
			}
		}
	}

	public bool HasDropLine
	{
		get
		{
			return m_hasDropLine;
		}
		set
		{
			m_hasDropLine = value;
			if (value)
			{
				m_usDropLines = 0;
			}
		}
	}

	public bool HasHighLowLine
	{
		get
		{
			return m_hasHighLowLine;
		}
		set
		{
			m_hasHighLowLine = value;
			if (value)
			{
				m_usDropLines = 1;
			}
		}
	}

	public bool HasSeriesLine
	{
		get
		{
			return m_hasSeriesLine;
		}
		set
		{
			m_hasSeriesLine = value;
			if (value)
			{
				m_usDropLines = 2;
			}
		}
	}

	public ChartChartLineRecord()
	{
	}

	public ChartChartLineRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartChartLineRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usDropLines = provider.ReadUInt16(iOffset);
		if (m_usDropLines == 0)
		{
			HasDropLine = true;
		}
		else if (m_usDropLines == 1)
		{
			HasHighLowLine = true;
		}
		else if (m_usDropLines == 2)
		{
			HasSeriesLine = true;
		}
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usDropLines);
		m_iLength = 2;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 2;
	}

	public static bool operator ==(ChartChartLineRecord record1, ChartChartLineRecord record2)
	{
		bool flag = object.Equals(record1, null);
		bool flag2 = object.Equals(record2, null);
		if (flag && flag2)
		{
			return true;
		}
		if (flag || flag2)
		{
			return false;
		}
		return record1.m_usDropLines == record2.m_usDropLines;
	}

	public static bool operator !=(ChartChartLineRecord record1, ChartChartLineRecord record2)
	{
		return !(record1 == record2);
	}
}
