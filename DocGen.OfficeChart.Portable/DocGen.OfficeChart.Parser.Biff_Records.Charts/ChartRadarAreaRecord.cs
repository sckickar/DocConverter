using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartRadarArea)]
[CLSCompliant(false)]
internal class ChartRadarAreaRecord : BiffRecordRaw
{
	public const int DefaultRecordSize = 4;

	[BiffRecordPos(0, 2)]
	private ushort m_usOptions;

	[BiffRecordPos(0, 0, TFieldType.Bit)]
	private bool m_bRadarAxisLabel;

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

	public ushort Resereved
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

	public ChartRadarAreaRecord()
	{
	}

	public ChartRadarAreaRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartRadarAreaRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usOptions = provider.ReadUInt16(iOffset);
		m_bRadarAxisLabel = provider.ReadBit(iOffset, 0);
		m_usReserved = provider.ReadUInt16(iOffset + 2);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usOptions);
		provider.WriteBit(iOffset, m_bRadarAxisLabel, 0);
		provider.WriteUInt16(iOffset + 2, m_usReserved);
		m_iLength = 4;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 4;
	}
}
