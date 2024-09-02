using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartAxisLineFormat)]
[CLSCompliant(false)]
internal class ChartAxisLineFormatRecord : BiffRecordRaw
{
	public const int DEF_RECORD_SIZE = 2;

	[BiffRecordPos(0, 2)]
	private ushort m_usAxisLineId;

	public ExcelAxisLineIdentifier LineIdentifier
	{
		get
		{
			return (ExcelAxisLineIdentifier)m_usAxisLineId;
		}
		set
		{
			m_usAxisLineId = (ushort)value;
		}
	}

	public override int MinimumRecordSize => 2;

	public override int MaximumRecordSize => 2;

	public ChartAxisLineFormatRecord()
	{
	}

	public ChartAxisLineFormatRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartAxisLineFormatRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usAxisLineId = provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_usAxisLineId &= 3;
		m_iLength = GetStoreSize(version);
		provider.WriteUInt16(iOffset, m_usAxisLineId);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 2;
	}
}
