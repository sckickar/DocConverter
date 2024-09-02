using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[CLSCompliant(false)]
[Biff(TBIFFRecord.PrintHeaders)]
internal class PrintHeadersRecord : BiffRecordRaw
{
	private const int DefaultRecordSize = 2;

	[BiffRecordPos(0, 2)]
	private ushort m_usPrintHeaders;

	public ushort IsPrintHeaders
	{
		get
		{
			return m_usPrintHeaders;
		}
		set
		{
			m_usPrintHeaders = value;
		}
	}

	public override int MinimumRecordSize => 2;

	public override int MaximumRecordSize => 2;

	public PrintHeadersRecord()
	{
	}

	public PrintHeadersRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public PrintHeadersRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usPrintHeaders = provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usPrintHeaders);
		m_iLength = 2;
	}
}
