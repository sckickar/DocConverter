using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.Begin)]
[CLSCompliant(false)]
internal class BeginRecord : BiffRecordRawWithArray
{
	public override int MaximumRecordSize => 0;

	public BeginRecord()
	{
	}

	public BeginRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public BeginRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
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
