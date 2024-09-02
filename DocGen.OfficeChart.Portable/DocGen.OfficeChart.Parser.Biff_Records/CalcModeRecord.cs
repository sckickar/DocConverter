using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.CalcMode)]
[CLSCompliant(false)]
internal class CalcModeRecord : BiffRecordRaw
{
	[BiffRecordPos(0, 2)]
	private ushort m_usCalcMode = 1;

	public OfficeCalculationMode CalculationMode
	{
		get
		{
			return (OfficeCalculationMode)m_usCalcMode;
		}
		set
		{
			m_usCalcMode = (ushort)value;
		}
	}

	public override int MinimumRecordSize => 2;

	public override int MaximumRecordSize => 2;

	public CalcModeRecord()
	{
	}

	public CalcModeRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public CalcModeRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usCalcMode = provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usCalcMode);
		m_iLength = 2;
	}
}
