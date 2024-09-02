using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.ChartUnits)]
[CLSCompliant(false)]
internal class ChartUnitsRecord : BiffRecordRaw
{
	public const int DEF_RECORD_SIZE = 2;

	[BiffRecordPos(0, 2)]
	private ushort m_usUnits;

	public ushort Units => m_usUnits;

	public override int MinimumRecordSize => 2;

	public override int MaximumRecordSize => 2;

	public ChartUnitsRecord()
	{
	}

	public ChartUnitsRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartUnitsRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usUnits = provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usUnits);
		m_iLength = 2;
	}
}
