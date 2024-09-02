using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.End)]
[CLSCompliant(false)]
internal class EndRecord : BiffRecordRawWithArray
{
	public override int MaximumRecordSize => 0;

	public EndRecord()
	{
	}

	public EndRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public EndRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure()
	{
		AutoExtractFields();
	}

	public override void InfillInternalData(OfficeVersion version)
	{
		m_iLength = 0;
	}
}
