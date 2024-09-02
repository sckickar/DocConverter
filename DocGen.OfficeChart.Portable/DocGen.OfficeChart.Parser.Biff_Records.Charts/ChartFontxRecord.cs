using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartFontx)]
[CLSCompliant(false)]
internal class ChartFontxRecord : BiffRecordRaw
{
	private const int DEF_RECORD_SIZE = 2;

	[BiffRecordPos(0, 2)]
	private ushort m_usFontIndex;

	public ushort FontIndex
	{
		get
		{
			return m_usFontIndex;
		}
		set
		{
			if (value != m_usFontIndex)
			{
				m_usFontIndex = value;
			}
		}
	}

	public override int MinimumRecordSize => 2;

	public override int MaximumRecordSize => 2;

	public ChartFontxRecord()
	{
	}

	public ChartFontxRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartFontxRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usFontIndex = provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usFontIndex);
		m_iLength = 2;
	}
}
