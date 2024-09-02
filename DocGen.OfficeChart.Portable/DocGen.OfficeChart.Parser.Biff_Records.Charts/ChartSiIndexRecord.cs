using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartSiIndex)]
[CLSCompliant(false)]
internal class ChartSiIndexRecord : BiffRecordRaw
{
	public const int DEF_RECORD_SIZE = 2;

	[BiffRecordPos(0, 2)]
	private ushort m_usNumIndex;

	public ushort NumIndex
	{
		get
		{
			return m_usNumIndex;
		}
		set
		{
			m_usNumIndex = value;
		}
	}

	public override int MinimumRecordSize => 2;

	public override int MaximumRecordSize => 2;

	public ChartSiIndexRecord()
	{
	}

	public ChartSiIndexRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartSiIndexRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usNumIndex = provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = GetStoreSize(version);
		provider.WriteUInt16(iOffset, m_usNumIndex);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 2;
	}
}
