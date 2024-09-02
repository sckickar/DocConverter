using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartFormatLink)]
[CLSCompliant(false)]
internal class ChartFormatLinkRecord : BiffRecordRaw
{
	public static readonly byte[] UNKNOWN_BYTES = new byte[10] { 0, 0, 0, 0, 0, 0, 0, 0, 15, 0 };

	public const int DefaultRecordSize = 10;

	public ChartFormatLinkRecord()
	{
	}

	public ChartFormatLinkRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartFormatLinkRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = 10;
		provider.WriteBytes(iOffset, UNKNOWN_BYTES, 0, m_iLength);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 10;
	}
}
