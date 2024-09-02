using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartChartFormat)]
[CLSCompliant(false)]
internal class ChartChartFormatRecord : BiffRecordRaw
{
	private const int DEF_RECORD_SIZE = 20;

	[BiffRecordPos(0, 4, true)]
	private int m_iReserved0;

	[BiffRecordPos(4, 4, true)]
	private int m_iReserved1;

	[BiffRecordPos(8, 4, true)]
	private int m_iReserved2;

	[BiffRecordPos(12, 4, true)]
	private int m_iReserved3;

	[BiffRecordPos(16, 2)]
	private ushort m_usOptions;

	[BiffRecordPos(16, 0, TFieldType.Bit)]
	private bool m_bIsVaryColor;

	[BiffRecordPos(18, 2)]
	private ushort m_usZOrder;

	public int Reserved0 => m_iReserved0;

	public int Reserved1 => m_iReserved1;

	public int Reserved2 => m_iReserved2;

	public int Reserved3 => m_iReserved3;

	public ushort Options => m_usOptions;

	public bool IsVaryColor
	{
		get
		{
			return m_bIsVaryColor;
		}
		set
		{
			m_bIsVaryColor = value;
		}
	}

	public ushort DrawingZOrder
	{
		get
		{
			return m_usZOrder;
		}
		set
		{
			if (value != m_usZOrder)
			{
				m_usZOrder = value;
			}
		}
	}

	public override int MinimumRecordSize => 20;

	public override int MaximumRecordSize => 20;

	public ChartChartFormatRecord()
	{
	}

	public ChartChartFormatRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartChartFormatRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_iReserved0 = provider.ReadInt32(iOffset);
		iOffset += 4;
		m_iReserved1 = provider.ReadInt32(iOffset);
		iOffset += 4;
		m_iReserved2 = provider.ReadInt32(iOffset);
		iOffset += 4;
		m_iReserved3 = provider.ReadInt32(iOffset);
		iOffset += 4;
		m_usOptions = provider.ReadUInt16(iOffset);
		m_bIsVaryColor = provider.ReadBit(iOffset, 0);
		iOffset += 2;
		m_usZOrder = provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iReserved0 = (m_iReserved1 = (m_iReserved2 = (m_iReserved3 = 0)));
		m_usOptions &= 1;
		m_iLength = GetStoreSize(version);
		provider.WriteInt32(iOffset, m_iReserved0);
		iOffset += 4;
		provider.WriteInt32(iOffset, m_iReserved1);
		iOffset += 4;
		provider.WriteInt32(iOffset, m_iReserved2);
		iOffset += 4;
		provider.WriteInt32(iOffset, m_iReserved3);
		iOffset += 4;
		provider.WriteUInt16(iOffset, m_usOptions);
		provider.WriteBit(iOffset, m_bIsVaryColor, 0);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usZOrder);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 20;
	}

	internal bool EqualsWithoutOrder(ChartChartFormatRecord chartFormatRecord)
	{
		if (m_usOptions == chartFormatRecord.m_usOptions)
		{
			return m_bIsVaryColor == chartFormatRecord.m_bIsVaryColor;
		}
		return false;
	}
}
