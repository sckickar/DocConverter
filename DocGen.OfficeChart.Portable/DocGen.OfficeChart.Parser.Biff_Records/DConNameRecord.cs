using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.DCONNAME)]
[CLSCompliant(false)]
internal class DConNameRecord : DConBinRecord
{
	public DConNameRecord()
	{
	}

	public DConNameRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public DConNameRecord(int iReserve)
		: base(iReserve)
	{
	}
}
