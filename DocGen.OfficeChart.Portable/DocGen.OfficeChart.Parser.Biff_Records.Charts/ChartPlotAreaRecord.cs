using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartPlotArea)]
[CLSCompliant(false)]
internal class ChartPlotAreaRecord : BiffRecordRaw
{
	public const int DefaultRecordSize = 0;

	public override int MinimumRecordSize => 0;

	public override int MaximumRecordSize => 0;

	public ChartPlotAreaRecord()
	{
	}

	public ChartPlotAreaRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartPlotAreaRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = 0;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 0;
	}
}
