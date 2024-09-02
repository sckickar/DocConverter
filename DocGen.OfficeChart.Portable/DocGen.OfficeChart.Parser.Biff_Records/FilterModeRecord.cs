using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.FilterMode)]
[CLSCompliant(false)]
internal class FilterModeRecord : BiffRecordRawWithArray
{
	public override int MaximumRecordSize => 0;

	public FilterModeRecord()
	{
	}

	public FilterModeRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public FilterModeRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure()
	{
	}

	public override void InfillInternalData(OfficeVersion version)
	{
		m_iLength = 0;
	}
}
