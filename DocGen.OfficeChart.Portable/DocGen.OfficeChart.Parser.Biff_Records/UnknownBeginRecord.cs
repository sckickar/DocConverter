using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.UnkBegin)]
[CLSCompliant(false)]
internal class UnknownBeginRecord : BiffRecordRaw
{
	public override int MaximumRecordSize => 0;

	public UnknownBeginRecord()
	{
	}

	public UnknownBeginRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public UnknownBeginRecord(int iReserve)
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
