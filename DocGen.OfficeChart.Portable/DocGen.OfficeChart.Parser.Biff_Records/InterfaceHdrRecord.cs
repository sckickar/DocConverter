using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.InterfaceHdr)]
[CLSCompliant(false)]
internal class InterfaceHdrRecord : BiffRecordRaw
{
	private const int DefaultRecordSize = 2;

	[BiffRecordPos(0, 2)]
	private ushort m_usCodepage = 1200;

	public ushort Codepage
	{
		get
		{
			return m_usCodepage;
		}
		set
		{
			m_usCodepage = value;
		}
	}

	public override int MinimumRecordSize => 2;

	public override int MaximumRecordSize => 2;

	public InterfaceHdrRecord()
	{
	}

	public InterfaceHdrRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public InterfaceHdrRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usCodepage = provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usCodepage);
		m_iLength = 2;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 2;
	}
}
