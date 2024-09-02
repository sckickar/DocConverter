using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.HCenter)]
[Biff(TBIFFRecord.VCenter)]
[CLSCompliant(false)]
internal class SheetCenterRecord : BiffRecordRaw
{
	[BiffRecordPos(0, 2)]
	private ushort m_usCenter;

	public ushort IsCenter
	{
		get
		{
			return m_usCenter;
		}
		set
		{
			m_usCenter = value;
		}
	}

	public override int MinimumRecordSize => 2;

	public override int MaximumRecordSize => 2;

	public SheetCenterRecord()
	{
	}

	public SheetCenterRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public SheetCenterRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usCenter = provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = GetStoreSize(version);
		provider.WriteUInt16(iOffset, m_usCenter);
	}
}
