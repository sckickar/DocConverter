using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.BookBool)]
[CLSCompliant(false)]
internal class BookBoolRecord : BiffRecordRaw
{
	[BiffRecordPos(0, 2)]
	private ushort m_usSaveLinkValue;

	public ushort SaveLinkValue
	{
		get
		{
			return m_usSaveLinkValue;
		}
		set
		{
			m_usSaveLinkValue = value;
		}
	}

	public override int MinimumRecordSize => 2;

	public override int MaximumRecordSize => 2;

	public BookBoolRecord()
	{
	}

	public BookBoolRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public BookBoolRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usSaveLinkValue = provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usSaveLinkValue);
		m_iLength = 2;
	}
}
