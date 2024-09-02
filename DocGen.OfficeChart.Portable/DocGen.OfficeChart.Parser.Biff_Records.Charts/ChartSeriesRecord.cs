using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartSeries)]
[CLSCompliant(false)]
internal class ChartSeriesRecord : BiffRecordRaw
{
	public enum DataType
	{
		Date,
		Numeric,
		Sequence,
		Text
	}

	public const int DEF_RECORD_SIZE = 12;

	[BiffRecordPos(0, 2)]
	private ushort m_usStdX;

	[BiffRecordPos(2, 2)]
	private ushort m_usStdY;

	[BiffRecordPos(4, 2)]
	private ushort m_usCatCount;

	[BiffRecordPos(6, 2)]
	private ushort m_usValCount;

	[BiffRecordPos(8, 2)]
	private ushort m_usBubbleDataType;

	[BiffRecordPos(10, 2)]
	private ushort m_usBubbleSeriesCount;

	public DataType StdX
	{
		get
		{
			return (DataType)m_usStdX;
		}
		set
		{
			m_usStdX = (ushort)value;
		}
	}

	public DataType StdY
	{
		get
		{
			return (DataType)m_usStdY;
		}
		set
		{
			m_usStdY = (ushort)value;
		}
	}

	public ushort CategoriesCount
	{
		get
		{
			return m_usCatCount;
		}
		set
		{
			m_usCatCount = value;
		}
	}

	public ushort ValuesCount
	{
		get
		{
			return m_usValCount;
		}
		set
		{
			m_usValCount = value;
		}
	}

	public DataType BubbleDataType
	{
		get
		{
			return (DataType)m_usBubbleDataType;
		}
		set
		{
			m_usBubbleDataType = (ushort)value;
		}
	}

	public ushort BubbleSeriesCount
	{
		get
		{
			return m_usBubbleSeriesCount;
		}
		set
		{
			m_usBubbleSeriesCount = value;
		}
	}

	public override int MinimumRecordSize => 12;

	public override int MaximumRecordSize => 12;

	public ChartSeriesRecord()
	{
	}

	public ChartSeriesRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartSeriesRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usStdX = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usStdY = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usCatCount = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usValCount = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usBubbleDataType = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usBubbleSeriesCount = provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = GetStoreSize(version);
		provider.WriteUInt16(iOffset, m_usStdX);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usStdY);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usCatCount);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usValCount);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usBubbleDataType);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usBubbleSeriesCount);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 12;
	}
}
