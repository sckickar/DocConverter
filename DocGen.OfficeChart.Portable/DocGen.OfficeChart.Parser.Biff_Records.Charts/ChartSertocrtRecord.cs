using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartSertocrt)]
[CLSCompliant(false)]
internal class ChartSertocrtRecord : BiffRecordRaw
{
	public const int DEF_RECORD_SIZE = 2;

	[BiffRecordPos(0, 2)]
	private ushort m_usChartGroup;

	public ushort ChartGroup
	{
		get
		{
			return m_usChartGroup;
		}
		set
		{
			m_usChartGroup = value;
		}
	}

	public override int MinimumRecordSize => 2;

	public override int MaximumRecordSize => 2;

	public ChartSertocrtRecord()
	{
	}

	public ChartSertocrtRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartSertocrtRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usChartGroup = provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = GetStoreSize(version);
		provider.WriteUInt16(iOffset, m_usChartGroup);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 2;
	}
}
