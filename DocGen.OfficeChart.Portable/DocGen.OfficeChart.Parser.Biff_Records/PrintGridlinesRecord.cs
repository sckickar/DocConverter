using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.PrintGridlines)]
[CLSCompliant(false)]
internal class PrintGridlinesRecord : BiffRecordRaw
{
	private const int DefaultRecordSize = 2;

	[BiffRecordPos(0, 2)]
	private ushort m_usPrintGridlines;

	public ushort IsPrintGridlines
	{
		get
		{
			return m_usPrintGridlines;
		}
		set
		{
			m_usPrintGridlines = value;
		}
	}

	public override int MinimumRecordSize => 2;

	public override int MaximumRecordSize => 2;

	public PrintGridlinesRecord()
	{
	}

	public PrintGridlinesRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public PrintGridlinesRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usPrintGridlines = provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usPrintGridlines);
		m_iLength = 2;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 2;
	}
}
