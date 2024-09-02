using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartAxisOffset)]
[CLSCompliant(false)]
internal class ChartAxisOffsetRecord : BiffRecordRaw
{
	public const int DEF_MIN_RECORD_SIZE = 10;

	public const int DEF_RECORD_SIZE = 12;

	[BiffRecordPos(4, 2)]
	private ushort m_usOffset;

	public int Offset
	{
		get
		{
			return m_usOffset;
		}
		set
		{
			m_usOffset = (ushort)value;
		}
	}

	public override int MinimumRecordSize => 10;

	public override int MaximumRecordSize => 12;

	public ChartAxisOffsetRecord()
	{
	}

	public ChartAxisOffsetRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartAxisOffsetRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		iOffset += 4;
		m_usOffset = provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, (ushort)base.TypeCode);
		iOffset += 2;
		provider.WriteUInt16(iOffset, 0);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usOffset);
		iOffset += 2;
		provider.WriteUInt16(iOffset, 2);
		iOffset += 2;
		provider.WriteUInt32(iOffset, 0u);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 12;
	}
}
