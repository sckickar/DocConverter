using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.CalCount)]
[CLSCompliant(false)]
internal class CalcCountRecord : BiffRecordRaw
{
	private const int DEF_RECORD_SIZE = 2;

	[BiffRecordPos(0, 2)]
	private ushort m_usIterations = 100;

	public ushort Iterations
	{
		get
		{
			return m_usIterations;
		}
		set
		{
			m_usIterations = value;
		}
	}

	public override int MinimumRecordSize => 2;

	public override int MaximumRecordSize => 2;

	public CalcCountRecord()
	{
	}

	public CalcCountRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public CalcCountRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usIterations = provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = 2;
		provider.WriteUInt16(iOffset, m_usIterations);
	}
}
