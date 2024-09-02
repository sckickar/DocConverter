using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.RecalcId)]
[CLSCompliant(false)]
internal class RecalcIdRecord : BiffRecordRaw
{
	[BiffRecordPos(0, 4)]
	private uint m_record = 449u;

	[BiffRecordPos(4, 8)]
	private uint m_dwBuild;

	public uint RecordId
	{
		get
		{
			return m_record;
		}
		set
		{
			m_record = value;
		}
	}

	public uint CalcIdentifier
	{
		get
		{
			return m_dwBuild;
		}
		set
		{
			m_dwBuild = value;
		}
	}

	public override int MinimumRecordSize => 8;

	public override int MaximumRecordSize => 8;

	public RecalcIdRecord()
	{
	}

	public RecalcIdRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public RecalcIdRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_record = provider.ReadUInt16(iOffset);
		m_dwBuild = provider.ReadUInt32(iOffset + 4);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt32(iOffset, m_record);
		provider.WriteUInt32(iOffset + 4, m_dwBuild);
	}
}
