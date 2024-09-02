using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartRadar)]
[CLSCompliant(false)]
internal class ChartRadarRecord : BiffRecordRaw
{
	public const int DEF_RECORD_SIZE = 4;

	[BiffRecordPos(0, 2)]
	private ushort m_usOptions;

	[BiffRecordPos(0, 0, TFieldType.Bit)]
	private bool m_bRadarAxisLabel;

	[BiffRecordPos(0, 1, TFieldType.Bit)]
	private bool m_bHasShadow;

	[BiffRecordPos(2, 2)]
	private ushort m_usReserved;

	public ushort Options => m_usOptions;

	public bool IsRadarAxisLabel
	{
		get
		{
			return m_bRadarAxisLabel;
		}
		set
		{
			m_bRadarAxisLabel = value;
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

	public ushort Reserved
	{
		get
		{
			return m_usReserved;
		}
		set
		{
			m_usReserved = value;
		}
	}

	public override int MinimumRecordSize => 4;

	public override int MaximumRecordSize => 4;

	public ChartRadarRecord()
	{
	}

	public ChartRadarRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartRadarRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usOptions = provider.ReadUInt16(iOffset);
		m_bRadarAxisLabel = provider.ReadBit(iOffset, 0);
		m_bHasShadow = provider.ReadBit(iOffset, 1);
		m_usReserved = provider.ReadUInt16(iOffset + 2);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usOptions);
		provider.WriteBit(iOffset, m_bRadarAxisLabel, 0);
		provider.WriteBit(iOffset, m_bHasShadow, 1);
		provider.WriteUInt16(iOffset + 2, m_usReserved);
		m_iLength = 4;
	}
}
