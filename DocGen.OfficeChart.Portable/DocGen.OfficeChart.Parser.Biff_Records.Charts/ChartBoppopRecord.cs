using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartBoppop)]
[CLSCompliant(false)]
internal class ChartBoppopRecord : BiffRecordRaw
{
	private const int DefaultRecordSize = 18;

	[BiffRecordPos(0, 1)]
	private byte m_PieType;

	[BiffRecordPos(1, 1)]
	private byte m_UseDefaultSplit;

	[BiffRecordPos(1, 0, TFieldType.Bit)]
	private bool m_bUseDefaultSplit;

	[BiffRecordPos(2, 2)]
	private ushort m_usSplitType;

	[BiffRecordPos(4, 2)]
	private ushort m_usSplitPos;

	[BiffRecordPos(6, 2)]
	private ushort m_usSplitPercent;

	[BiffRecordPos(8, 2)]
	private ushort m_usPie2Size;

	[BiffRecordPos(10, 2)]
	private ushort m_usGap;

	[BiffRecordPos(12, 4, true)]
	private int m_uiNumSplitValue;

	[BiffRecordPos(16, 2)]
	private ushort m_usHasShadow;

	[BiffRecordPos(16, 0, TFieldType.Bit)]
	private bool m_bHasShadow;

	private bool m_bShowLeaderLines;

	public OfficePieType PieChartType
	{
		get
		{
			return (OfficePieType)m_PieType;
		}
		set
		{
			m_PieType = (byte)value;
		}
	}

	public bool UseDefaultSplitValue
	{
		get
		{
			return m_bUseDefaultSplit;
		}
		set
		{
			m_bUseDefaultSplit = value;
		}
	}

	public OfficeSplitType ChartSplitType
	{
		get
		{
			return (OfficeSplitType)m_usSplitType;
		}
		set
		{
			m_usSplitType = (ushort)value;
		}
	}

	public ushort SplitPosition
	{
		get
		{
			return m_usSplitPos;
		}
		set
		{
			if (value != m_usSplitPos)
			{
				m_usSplitPos = value;
			}
		}
	}

	public ushort SplitPercent
	{
		get
		{
			return m_usSplitPercent;
		}
		set
		{
			if (value != m_usSplitPercent)
			{
				m_usSplitPercent = value;
			}
		}
	}

	public ushort Pie2Size
	{
		get
		{
			return m_usPie2Size;
		}
		set
		{
			if (value != m_usPie2Size)
			{
				m_usPie2Size = value;
			}
		}
	}

	public ushort Gap
	{
		get
		{
			return m_usGap;
		}
		set
		{
			if (value != m_usGap)
			{
				m_usGap = value;
			}
		}
	}

	public int NumSplitValue
	{
		get
		{
			return m_uiNumSplitValue;
		}
		set
		{
			if (value != m_uiNumSplitValue)
			{
				m_uiNumSplitValue = value;
			}
		}
	}

	public bool HasShadow
	{
		get
		{
			return m_bHasShadow;
		}
		set
		{
			m_bHasShadow = value;
		}
	}

	public bool ShowLeaderLines
	{
		get
		{
			return m_bShowLeaderLines;
		}
		set
		{
			m_bShowLeaderLines = value;
		}
	}

	public ChartBoppopRecord()
	{
	}

	public ChartBoppopRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartBoppopRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_PieType = provider.ReadByte(iOffset);
		m_UseDefaultSplit = provider.ReadByte(iOffset + 1);
		m_bUseDefaultSplit = provider.ReadBit(iOffset + 1, 0);
		m_usSplitType = provider.ReadUInt16(iOffset + 2);
		m_usSplitPos = provider.ReadUInt16(iOffset + 4);
		m_usSplitPercent = provider.ReadUInt16(iOffset + 6);
		m_usPie2Size = provider.ReadUInt16(iOffset + 8);
		m_usGap = provider.ReadUInt16(iOffset + 10);
		m_uiNumSplitValue = provider.ReadInt32(iOffset + 12);
		m_usHasShadow = provider.ReadUInt16(iOffset + 16);
		m_bHasShadow = provider.ReadBit(iOffset + 16, 0);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_usHasShadow &= 1;
		m_UseDefaultSplit &= 1;
		provider.WriteByte(iOffset, m_PieType);
		provider.WriteByte(iOffset + 1, m_UseDefaultSplit);
		provider.WriteBit(iOffset + 1, m_bUseDefaultSplit, 0);
		provider.WriteUInt16(iOffset + 2, m_usSplitType);
		provider.WriteUInt16(iOffset + 4, m_usSplitPos);
		provider.WriteUInt16(iOffset + 6, m_usSplitPercent);
		provider.WriteUInt16(iOffset + 8, m_usPie2Size);
		provider.WriteUInt16(iOffset + 10, m_usGap);
		provider.WriteInt32(iOffset + 12, m_uiNumSplitValue);
		provider.WriteUInt16(iOffset + 16, m_usHasShadow);
		provider.WriteBit(iOffset + 16, m_bHasShadow, 0);
		m_iLength = 18;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 18;
	}
}
