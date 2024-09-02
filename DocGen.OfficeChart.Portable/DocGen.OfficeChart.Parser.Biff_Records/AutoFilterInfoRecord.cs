using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.AutoFilterInfo)]
[CLSCompliant(false)]
internal class AutoFilterInfoRecord : BiffRecordRaw
{
	private const int DEF_RECORD_SIZE = 2;

	[BiffRecordPos(0, 2)]
	private ushort m_usArrowsCount;

	public override bool NeedDataArray => true;

	public override int MaximumRecordSize => 2;

	public override int MinimumRecordSize => 2;

	public ushort ArrowsCount
	{
		get
		{
			return m_usArrowsCount;
		}
		set
		{
			m_usArrowsCount = value;
		}
	}

	public AutoFilterInfoRecord()
	{
	}

	public AutoFilterInfoRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public AutoFilterInfoRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usArrowsCount = provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usArrowsCount);
		m_iLength = 2;
	}
}
