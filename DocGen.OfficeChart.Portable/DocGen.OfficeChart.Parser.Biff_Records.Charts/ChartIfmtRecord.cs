using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartIfmt)]
[CLSCompliant(false)]
internal class ChartIfmtRecord : BiffRecordRaw
{
	private const int DefaultRecordSize = 2;

	[BiffRecordPos(0, 2)]
	private ushort m_usNumberIndex;

	public ushort FormatIndex
	{
		get
		{
			return m_usNumberIndex;
		}
		set
		{
			if (value != m_usNumberIndex)
			{
				m_usNumberIndex = value;
			}
		}
	}

	public ChartIfmtRecord()
	{
	}

	public ChartIfmtRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartIfmtRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usNumberIndex = provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usNumberIndex);
		m_iLength = 2;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 2;
	}
}
