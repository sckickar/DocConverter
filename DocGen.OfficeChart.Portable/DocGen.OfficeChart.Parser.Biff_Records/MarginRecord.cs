using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.TopMargin)]
[Biff(TBIFFRecord.LeftMargin)]
[Biff(TBIFFRecord.BottomMargin)]
[Biff(TBIFFRecord.RightMargin)]
[CLSCompliant(false)]
internal class MarginRecord : BiffRecordRaw
{
	public const double DEFAULT_VALUE = 0.0;

	private const int DEF_RECORD_SIZE = 8;

	[BiffRecordPos(0, 8, TFieldType.Float)]
	private double m_dbMargin;

	public double Margin
	{
		get
		{
			return m_dbMargin;
		}
		set
		{
			m_dbMargin = value;
		}
	}

	public override int MinimumRecordSize => 8;

	public override int MaximumRecordSize => 8;

	public MarginRecord()
	{
	}

	public MarginRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public MarginRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_dbMargin = provider.ReadDouble(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = 8;
		provider.WriteDouble(iOffset, m_dbMargin);
	}
}
