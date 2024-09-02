using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartDataFormat)]
[CLSCompliant(false)]
internal class ChartDataFormatRecord : BiffRecordRaw
{
	public const int DEF_RECORD_SIZE = 8;

	[BiffRecordPos(0, 2)]
	private ushort m_usPointNumber;

	[BiffRecordPos(2, 2)]
	private ushort m_usSeriesIndex;

	[BiffRecordPos(4, 2)]
	private ushort m_usSeriesNumber;

	[BiffRecordPos(6, 2)]
	private ushort m_usOptions;

	[BiffRecordPos(6, 0, TFieldType.Bit)]
	private bool m_bUseXL4Color;

	public ushort PointNumber
	{
		get
		{
			return m_usPointNumber;
		}
		set
		{
			if (value != m_usPointNumber)
			{
				m_usPointNumber = value;
			}
		}
	}

	public ushort SeriesIndex
	{
		get
		{
			return m_usSeriesIndex;
		}
		set
		{
			if (value != m_usSeriesIndex)
			{
				m_usSeriesIndex = value;
			}
		}
	}

	public ushort SeriesNumber
	{
		get
		{
			return m_usSeriesNumber;
		}
		set
		{
			if (value != m_usSeriesNumber)
			{
				m_usSeriesNumber = value;
			}
		}
	}

	public ushort Options => m_usOptions;

	public bool UserExcel4Colors
	{
		get
		{
			return m_bUseXL4Color;
		}
		set
		{
			m_bUseXL4Color = value;
		}
	}

	public override int MinimumRecordSize => 8;

	public override int MaximumRecordSize => 8;

	public ChartDataFormatRecord()
	{
	}

	public ChartDataFormatRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartDataFormatRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usPointNumber = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usSeriesIndex = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usSeriesNumber = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usOptions = provider.ReadUInt16(iOffset);
		m_bUseXL4Color = provider.ReadBit(iOffset, 0);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_usOptions &= 1;
		m_iLength = GetStoreSize(version);
		provider.WriteUInt16(iOffset, m_usPointNumber);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usSeriesIndex);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usSeriesNumber);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usOptions);
		provider.WriteBit(iOffset, m_bUseXL4Color, 0);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 8;
	}
}
