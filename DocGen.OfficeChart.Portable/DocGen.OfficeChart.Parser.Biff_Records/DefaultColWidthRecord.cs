using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.DefaultColWidth)]
[CLSCompliant(false)]
internal class DefaultColWidthRecord : BiffRecordRaw
{
	private const int DefaultRecordSize = 2;

	[BiffRecordPos(0, 2)]
	private ushort m_usColWidth = 8;

	public ushort Width
	{
		get
		{
			return m_usColWidth;
		}
		set
		{
			m_usColWidth = value;
		}
	}

	public override int MinimumRecordSize => 2;

	public override int MaximumRecordSize => 2;

	public DefaultColWidthRecord()
	{
	}

	public DefaultColWidthRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public DefaultColWidthRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usColWidth = provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usColWidth);
		m_iLength = 2;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 2;
	}
}
