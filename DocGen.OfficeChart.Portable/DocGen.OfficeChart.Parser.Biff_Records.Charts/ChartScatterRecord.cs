using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartScatter)]
[CLSCompliant(false)]
internal class ChartScatterRecord : BiffRecordRaw
{
	public const int DefaultRecordSize = 6;

	[BiffRecordPos(0, 2)]
	private ushort m_usBubleSizeRation;

	[BiffRecordPos(2, 2)]
	private ushort m_usBubleSize;

	[BiffRecordPos(4, 2)]
	private ushort m_usOptions;

	[BiffRecordPos(4, 0, TFieldType.Bit)]
	private bool m_bBubbles;

	[BiffRecordPos(4, 1, TFieldType.Bit)]
	private bool m_bShowNegBubbles;

	[BiffRecordPos(4, 2, TFieldType.Bit)]
	private bool m_bHasShadow;

	public override int MinimumRecordSize => 6;

	public override int MaximumRecordSize => 6;

	public ushort BubleSizeRation
	{
		get
		{
			return m_usBubleSizeRation;
		}
		set
		{
			m_usBubleSizeRation = value;
		}
	}

	public ChartBubbleSize BubleSize
	{
		get
		{
			return (ChartBubbleSize)m_usBubleSize;
		}
		set
		{
			m_usBubleSize = (ushort)value;
		}
	}

	public ushort Options
	{
		get
		{
			return m_usOptions;
		}
		set
		{
			m_usOptions = value;
		}
	}

	public bool IsBubbles
	{
		get
		{
			return m_bBubbles;
		}
		set
		{
			m_bBubbles = value;
		}
	}

	public bool IsShowNegBubbles
	{
		get
		{
			return m_bShowNegBubbles;
		}
		set
		{
			m_bShowNegBubbles = value;
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

	public ChartScatterRecord()
	{
	}

	public ChartScatterRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartScatterRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usBubleSizeRation = provider.ReadUInt16(iOffset);
		m_usBubleSize = provider.ReadUInt16(iOffset + 2);
		m_usOptions = provider.ReadUInt16(iOffset + 4);
		m_bBubbles = provider.ReadBit(iOffset + 4, 0);
		m_bShowNegBubbles = provider.ReadBit(iOffset + 4, 1);
		m_bHasShadow = provider.ReadBit(iOffset + 4, 2);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usBubleSizeRation);
		provider.WriteUInt16(iOffset + 2, m_usBubleSize);
		provider.WriteUInt16(iOffset + 4, m_usOptions);
		provider.WriteBit(iOffset + 4, m_bBubbles, 0);
		provider.WriteBit(iOffset + 4, m_bShowNegBubbles, 1);
		provider.WriteBit(iOffset + 4, m_bHasShadow, 2);
		m_iLength = 6;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 6;
	}
}
