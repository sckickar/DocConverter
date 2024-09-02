using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.Delta)]
[CLSCompliant(false)]
internal class DeltaRecord : BiffRecordRaw
{
	public const double DEFAULT_VALUE = 0.001;

	private const int DEF_RECORD_SIZE = 8;

	[BiffRecordPos(0, 8, TFieldType.Float)]
	private double m_dbMaxChange = 0.001;

	public double MaxChange
	{
		get
		{
			return m_dbMaxChange;
		}
		set
		{
			m_dbMaxChange = value;
		}
	}

	public override int MinimumRecordSize => 8;

	public override int MaximumRecordSize => 8;

	public DeltaRecord()
	{
	}

	public DeltaRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public DeltaRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_dbMaxChange = provider.ReadDouble(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = 8;
		provider.WriteDouble(iOffset, m_dbMaxChange);
	}
}
