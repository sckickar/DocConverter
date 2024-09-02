using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.InterfaceEnd)]
[CLSCompliant(false)]
internal class InterfaceEndRecord : BiffRecordRawWithArray
{
	public override int MinimumRecordSize => 0;

	public override int MaximumRecordSize => 0;

	public InterfaceEndRecord()
	{
	}

	public InterfaceEndRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public InterfaceEndRecord(int iReserve)
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
