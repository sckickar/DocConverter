using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartPie)]
[CLSCompliant(false)]
internal class ChartPieRecord : BiffRecordRaw
{
	public const int DefaultRecordSize = 6;

	[BiffRecordPos(0, 2)]
	private ushort m_usStartAngle;

	[BiffRecordPos(2, 2)]
	private ushort m_usDonutHoleSize;

	[BiffRecordPos(4, 2)]
	private ushort m_usOptions;

	[BiffRecordPos(4, 0, TFieldType.Bit)]
	private bool m_bHasShadow;

	[BiffRecordPos(4, 1, TFieldType.Bit)]
	private bool m_bShowLeaderLines;

	public ushort StartAngle
	{
		get
		{
			return m_usStartAngle;
		}
		set
		{
			m_usStartAngle = value;
		}
	}

	public ushort DonutHoleSize
	{
		get
		{
			return m_usDonutHoleSize;
		}
		set
		{
			m_usDonutHoleSize = value;
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

	public override int MinimumRecordSize => 6;

	public override int MaximumRecordSize => 6;

	public ChartPieRecord()
	{
	}

	public ChartPieRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartPieRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usStartAngle = provider.ReadUInt16(iOffset);
		m_usDonutHoleSize = provider.ReadUInt16(iOffset + 2);
		m_usOptions = provider.ReadUInt16(iOffset + 4);
		m_bHasShadow = provider.ReadBit(iOffset + 4, 0);
		m_bShowLeaderLines = provider.ReadBit(iOffset + 4, 1);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usStartAngle);
		provider.WriteUInt16(iOffset + 2, m_usDonutHoleSize);
		provider.WriteUInt16(iOffset + 4, m_usOptions);
		provider.WriteBit(iOffset + 4, m_bHasShadow, 0);
		provider.WriteBit(iOffset + 4, m_bShowLeaderLines, 1);
		m_iLength = 6;
	}
}
