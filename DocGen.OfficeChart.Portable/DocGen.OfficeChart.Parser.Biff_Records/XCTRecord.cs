using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[CLSCompliant(false)]
[Biff(TBIFFRecord.XCT)]
internal class XCTRecord : BiffRecordRaw
{
	private const int DEF_RECORD_SIZE = 4;

	[BiffRecordPos(0, 2)]
	private ushort m_usCRNCount;

	[BiffRecordPos(2, 2)]
	private ushort m_usSheetTableIndex;

	public ushort CRNCount
	{
		get
		{
			return m_usCRNCount;
		}
		set
		{
			m_usCRNCount = value;
		}
	}

	public ushort SheetTableIndex
	{
		get
		{
			return m_usSheetTableIndex;
		}
		set
		{
			m_usSheetTableIndex = value;
		}
	}

	public override int MinimumRecordSize => 4;

	public override int MaximumRecordSize => 4;

	public XCTRecord()
	{
	}

	public XCTRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public XCTRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usCRNCount = provider.ReadUInt16(iOffset);
		m_usSheetTableIndex = provider.ReadUInt16(iOffset + 2);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = 4;
		provider.WriteUInt16(iOffset, m_usCRNCount);
		provider.WriteUInt16(iOffset + 2, m_usSheetTableIndex);
	}
}
