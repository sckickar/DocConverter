using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartShtprops)]
[CLSCompliant(false)]
internal class ChartShtpropsRecord : BiffRecordRaw
{
	public const int DEF_RECORD_SIZE = 4;

	public const int DEF_MIN_RECORD_SIZE = 3;

	[BiffRecordPos(0, 2)]
	private ushort m_usFlags;

	[BiffRecordPos(0, 0, TFieldType.Bit)]
	private bool m_bManSerAlloc;

	[BiffRecordPos(0, 1, TFieldType.Bit)]
	private bool m_bPlotVisOnly = true;

	[BiffRecordPos(0, 2, TFieldType.Bit)]
	private bool m_bNotSizeWith = true;

	[BiffRecordPos(0, 3, TFieldType.Bit)]
	private bool m_bManPlotArea = true;

	[BiffRecordPos(0, 4, TFieldType.Bit)]
	private bool m_bAlwaysAutoPlotArea;

	[BiffRecordPos(2, 1)]
	private byte m_plotBlank;

	[BiffRecordPos(3, 1)]
	private byte m_notUsed;

	public ushort Flags => m_usFlags;

	public bool IsManSerAlloc
	{
		get
		{
			return m_bManSerAlloc;
		}
		set
		{
			m_bManSerAlloc = value;
		}
	}

	public bool IsPlotVisOnly
	{
		get
		{
			return m_bPlotVisOnly;
		}
		set
		{
			m_bPlotVisOnly = value;
		}
	}

	public bool IsNotSizeWith
	{
		get
		{
			return m_bNotSizeWith;
		}
		set
		{
			m_bNotSizeWith = value;
		}
	}

	public bool IsManPlotArea
	{
		get
		{
			return m_bManPlotArea;
		}
		set
		{
			m_bManPlotArea = value;
		}
	}

	public bool IsAlwaysAutoPlotArea
	{
		get
		{
			return m_bAlwaysAutoPlotArea;
		}
		set
		{
			m_bAlwaysAutoPlotArea = value;
		}
	}

	public OfficeChartPlotEmpty PlotBlank
	{
		get
		{
			return (OfficeChartPlotEmpty)m_plotBlank;
		}
		set
		{
			m_plotBlank = (byte)value;
		}
	}

	public byte Reserved
	{
		get
		{
			return m_notUsed;
		}
		set
		{
			m_notUsed = value;
		}
	}

	public override int MinimumRecordSize => 3;

	public override int MaximumRecordSize => 4;

	public ChartShtpropsRecord()
	{
	}

	public ChartShtpropsRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartShtpropsRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usFlags = provider.ReadUInt16(iOffset);
		m_bManSerAlloc = provider.ReadBit(iOffset, 0);
		m_bPlotVisOnly = provider.ReadBit(iOffset, 1);
		m_bNotSizeWith = provider.ReadBit(iOffset, 2);
		m_bManPlotArea = provider.ReadBit(iOffset, 3);
		m_bAlwaysAutoPlotArea = provider.ReadBit(iOffset, 4);
		m_plotBlank = provider.ReadByte(iOffset + 2);
		if (iLength > 3)
		{
			m_notUsed = provider.ReadByte(iOffset + 3);
		}
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usFlags);
		provider.WriteBit(iOffset, m_bManSerAlloc, 0);
		provider.WriteBit(iOffset, m_bPlotVisOnly, 1);
		provider.WriteBit(iOffset, m_bNotSizeWith, 2);
		provider.WriteBit(iOffset, m_bManPlotArea, 3);
		provider.WriteBit(iOffset, m_bAlwaysAutoPlotArea, 4);
		iOffset += 2;
		provider.WriteByte(iOffset, m_plotBlank);
		iOffset++;
		provider.WriteByte(iOffset, m_notUsed);
		m_iLength = GetStoreSize(version);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 4;
	}
}
