using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[CLSCompliant(false)]
[Biff(TBIFFRecord.WriteProtection)]
internal class WriteProtection : BiffRecordRaw
{
	private const int DEF_RECORD_SIZE = 0;

	public override int MaximumRecordSize => 0;

	public WriteProtection()
	{
	}

	public WriteProtection(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public WriteProtection(int iReserve)
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
