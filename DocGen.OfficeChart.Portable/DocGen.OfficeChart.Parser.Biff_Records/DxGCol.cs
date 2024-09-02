using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.DxGCol)]
[CLSCompliant(false)]
internal class DxGCol : BiffRecordRaw
{
	private const int DEF_RECORD_SIZE = 2;

	[BiffRecordPos(0, 2)]
	private ushort m_usColWidth = 2340;

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

	public DxGCol()
	{
	}

	public DxGCol(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public DxGCol(int iReserve)
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
