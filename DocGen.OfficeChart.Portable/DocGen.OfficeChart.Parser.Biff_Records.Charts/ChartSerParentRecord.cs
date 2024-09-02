using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartSerParent)]
[CLSCompliant(false)]
internal class ChartSerParentRecord : BiffRecordRaw
{
	public const int DEF_RECORD_SIZE = 2;

	[BiffRecordPos(0, 2)]
	private ushort m_usSeries;

	public ushort Series
	{
		get
		{
			return m_usSeries;
		}
		set
		{
			m_usSeries = value;
		}
	}

	public override int MinimumRecordSize => 2;

	public override int MaximumRecordSize => 2;

	public ChartSerParentRecord()
	{
	}

	public ChartSerParentRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartSerParentRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usSeries = provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usSeries);
		m_iLength = 2;
	}
}
