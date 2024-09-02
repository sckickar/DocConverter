using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartPieFormat)]
[CLSCompliant(false)]
internal class ChartPieFormatRecord : BiffRecordRaw
{
	public const int DEF_RECORD_SIZE = 2;

	[BiffRecordPos(0, 2)]
	private ushort m_usPercent;

	public ushort Percent
	{
		get
		{
			return m_usPercent;
		}
		set
		{
			m_usPercent = value;
		}
	}

	public override int MinimumRecordSize => 2;

	public override int MaximumRecordSize => 2;

	public ChartPieFormatRecord()
	{
	}

	public ChartPieFormatRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartPieFormatRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usPercent = provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = GetStoreSize(version);
		provider.WriteUInt16(iOffset, m_usPercent);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 2;
	}
}
