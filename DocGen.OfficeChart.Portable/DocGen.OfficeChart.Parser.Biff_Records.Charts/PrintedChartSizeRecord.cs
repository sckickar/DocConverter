using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.PrintedChartSize)]
[CLSCompliant(false)]
internal class PrintedChartSizeRecord : BiffRecordRaw
{
	public const int DefaultRecordSize = 2;

	[BiffRecordPos(0, 2)]
	private ushort m_prnChartSize = 3;

	public OfficePrintedChartSize PrintedChartSize
	{
		get
		{
			return (OfficePrintedChartSize)m_prnChartSize;
		}
		set
		{
			m_prnChartSize = (ushort)value;
		}
	}

	public override int MinimumRecordSize => 2;

	public override int MaximumRecordSize => 2;

	public PrintedChartSizeRecord()
	{
	}

	public PrintedChartSizeRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public PrintedChartSizeRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_prnChartSize = provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_prnChartSize);
		m_iLength = 2;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 2;
	}
}
