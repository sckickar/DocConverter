using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.MMS)]
[CLSCompliant(false)]
internal class MMSRecord : BiffRecordRaw
{
	private const int DefaultRecordSize = 2;

	[BiffRecordPos(0, 1)]
	private byte m_AddMenuCount;

	[BiffRecordPos(1, 1)]
	private byte m_DelMenuCount;

	public byte AddMenuCount
	{
		get
		{
			return m_AddMenuCount;
		}
		set
		{
			m_AddMenuCount = value;
		}
	}

	public byte DelMenuCount
	{
		get
		{
			return m_DelMenuCount;
		}
		set
		{
			m_DelMenuCount = value;
		}
	}

	public override int MinimumRecordSize => 2;

	public override int MaximumRecordSize => 2;

	public MMSRecord()
	{
	}

	public MMSRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public MMSRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_AddMenuCount = provider.ReadByte(iOffset);
		m_DelMenuCount = provider.ReadByte(iOffset + 1);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteByte(iOffset, m_AddMenuCount);
		provider.WriteByte(iOffset + 1, m_DelMenuCount);
		m_iLength = 2;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 2;
	}
}
