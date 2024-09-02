using System;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.Number)]
[CLSCompliant(false)]
internal class NumberRecord : CellPositionBase, IDoubleValue, IValueHolder
{
	private const int DEF_RECORD_SIZE = 14;

	[BiffRecordPos(6, 8, TFieldType.Float)]
	private double m_dbValue;

	public double Value
	{
		get
		{
			return m_dbValue;
		}
		set
		{
			m_dbValue = value;
		}
	}

	public override int MinimumRecordSize => 14;

	public override int MaximumRecordSize => 14;

	public override int MaximumMemorySize => 14;

	public double DoubleValue => m_dbValue;

	object IValueHolder.Value
	{
		get
		{
			return m_dbValue;
		}
		set
		{
			m_dbValue = (double)value;
		}
	}

	protected override void ParseCellData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_dbValue = provider.ReadDouble(iOffset);
	}

	protected override void InfillCellData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteDouble(iOffset, m_dbValue);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		int num = 14;
		if (version != 0)
		{
			num += 4;
		}
		return num;
	}

	public static double ReadValue(DataProvider provider, int recordStart, OfficeVersion version)
	{
		recordStart += 10;
		if (version != 0)
		{
			recordStart += 4;
		}
		return provider.ReadDouble(recordStart);
	}
}
