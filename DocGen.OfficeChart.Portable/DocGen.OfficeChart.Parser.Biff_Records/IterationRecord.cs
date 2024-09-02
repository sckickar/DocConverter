using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.Iteration)]
[CLSCompliant(false)]
internal class IterationRecord : BiffRecordRaw
{
	private const int DEF_RECORD_SIZE = 2;

	[BiffRecordPos(0, 2)]
	private ushort m_usIteration;

	public ushort IsIteration
	{
		get
		{
			return m_usIteration;
		}
		set
		{
			m_usIteration = value;
		}
	}

	public override int MinimumRecordSize => 2;

	public override int MaximumRecordSize => 2;

	public IterationRecord()
	{
	}

	public IterationRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public IterationRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usIteration = provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = 2;
		provider.WriteUInt16(iOffset, m_usIteration);
	}
}
