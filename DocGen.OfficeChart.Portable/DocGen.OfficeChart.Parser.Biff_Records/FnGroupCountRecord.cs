using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.FnGroupCount)]
[CLSCompliant(false)]
internal class FnGroupCountRecord : BiffRecordRaw
{
	private const int DefaultRecordSize = 2;

	[BiffRecordPos(0, 2)]
	private ushort m_usCount = 14;

	public ushort Count
	{
		get
		{
			return m_usCount;
		}
		set
		{
			m_usCount = value;
		}
	}

	public override int MinimumRecordSize => 2;

	public override int MaximumRecordSize => 2;

	public FnGroupCountRecord()
	{
	}

	public FnGroupCountRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public FnGroupCountRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usCount = provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usCount);
		m_iLength = 2;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 2;
	}
}
