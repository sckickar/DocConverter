using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.HasBasic)]
[CLSCompliant(false)]
internal class HasBasicRecord : BiffRecordRaw
{
	public override int MaximumRecordSize => 0;

	public HasBasicRecord()
	{
	}

	public HasBasicRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public HasBasicRecord(int iReserve)
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
}
