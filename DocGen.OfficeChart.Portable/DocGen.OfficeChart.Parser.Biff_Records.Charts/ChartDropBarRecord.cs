using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartDropBar)]
[CLSCompliant(false)]
internal class ChartDropBarRecord : BiffRecordRaw
{
	private const int DefaultRecordSize = 2;

	[BiffRecordPos(0, 2)]
	private ushort m_usGap;

	public ushort Gap
	{
		get
		{
			return m_usGap;
		}
		set
		{
			if (value < 0 || value > 500)
			{
				throw new ArgumentOutOfRangeException("m_usGap", "Value cannot be less than 0 or greater than 100.");
			}
			if (value != m_usGap)
			{
				m_usGap = value;
			}
		}
	}

	public ChartDropBarRecord()
	{
	}

	public ChartDropBarRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartDropBarRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usGap = provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usGap);
		m_iLength = 2;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 2;
	}
}
