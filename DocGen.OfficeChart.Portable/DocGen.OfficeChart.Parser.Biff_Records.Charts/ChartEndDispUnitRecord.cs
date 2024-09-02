using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartEndDispUnit)]
[CLSCompliant(false)]
internal class ChartEndDispUnitRecord : BiffRecordRaw
{
	public const int DEF_MIN_RECORD_SIZE = 6;

	public const int DEF_RECORD_SIZE = 12;

	[BiffRecordPos(4, 4, TFieldType.Bit)]
	private bool m_bIsShowLabel;

	public bool IsShowLabels
	{
		get
		{
			return m_bIsShowLabel;
		}
		set
		{
			m_bIsShowLabel = value;
		}
	}

	public override int MinimumRecordSize => 6;

	public override int MaximumRecordSize => 12;

	public ChartEndDispUnitRecord()
	{
	}

	public ChartEndDispUnitRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartEndDispUnitRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		iOffset += 4;
		m_bIsShowLabel = provider.ReadBit(iOffset, 4);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, (ushort)base.TypeCode);
		iOffset += 2;
		provider.WriteUInt16(iOffset, 0);
		iOffset += 2;
		int offset = iOffset;
		provider.WriteUInt32(iOffset, 0u);
		iOffset += 4;
		provider.WriteUInt32(iOffset, 0u);
		iOffset += 4;
		provider.WriteBit(offset, m_bIsShowLabel, 4);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 12;
	}
}
