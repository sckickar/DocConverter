using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartLegendxn)]
[CLSCompliant(false)]
internal class ChartLegendxnRecord : BiffRecordRaw
{
	private const int DefaultRecordSize = 4;

	[BiffRecordPos(0, 2)]
	private ushort m_usLegendEntityIndex = ushort.MaxValue;

	[BiffRecordPos(2, 2)]
	private ushort m_usOptions;

	[BiffRecordPos(2, 0, TFieldType.Bit)]
	private bool m_bIsDeleted;

	[BiffRecordPos(2, 1, TFieldType.Bit)]
	private bool m_bIsFormatted;

	public ushort LegendEntityIndex
	{
		get
		{
			return m_usLegendEntityIndex;
		}
		set
		{
			if (value != m_usLegendEntityIndex)
			{
				m_usLegendEntityIndex = value;
			}
		}
	}

	public ushort Options => m_usOptions;

	public bool IsDeleted
	{
		get
		{
			return m_bIsDeleted;
		}
		set
		{
			m_bIsDeleted = value;
		}
	}

	public bool IsFormatted
	{
		get
		{
			return m_bIsFormatted;
		}
		set
		{
			m_bIsFormatted = value;
		}
	}

	public ChartLegendxnRecord()
	{
	}

	public ChartLegendxnRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartLegendxnRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usLegendEntityIndex = provider.ReadUInt16(iOffset);
		m_usOptions = provider.ReadUInt16(iOffset + 2);
		m_bIsDeleted = provider.ReadBit(iOffset + 2, 0);
		m_bIsFormatted = provider.ReadBit(iOffset + 2, 1);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_usOptions &= 3;
		provider.WriteUInt16(iOffset, m_usLegendEntityIndex);
		provider.WriteUInt16(iOffset + 2, m_usOptions);
		provider.WriteBit(iOffset + 2, m_bIsDeleted, 0);
		provider.WriteBit(iOffset + 2, m_bIsFormatted, 1);
		m_iLength = 4;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 4;
	}
}
