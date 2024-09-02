using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.EOF)]
[CLSCompliant(false)]
internal class EOFRecord : BiffRecordRaw
{
	public override int MaximumRecordSize => 0;

	public EOFRecord()
	{
	}

	public EOFRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public EOFRecord(int iReserve)
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
