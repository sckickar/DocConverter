using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartAxesUsed)]
[CLSCompliant(false)]
internal class ChartAxesUsedRecord : BiffRecordRaw
{
	[BiffRecordPos(0, 2)]
	private ushort m_usAxes;

	public ushort NumberOfAxes
	{
		get
		{
			return m_usAxes;
		}
		set
		{
			if (value != m_usAxes)
			{
				m_usAxes = value;
			}
		}
	}

	public ChartAxesUsedRecord()
	{
	}

	public ChartAxesUsedRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartAxesUsedRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usAxes = provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usAxes);
		m_iLength = 2;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 2;
	}
}
