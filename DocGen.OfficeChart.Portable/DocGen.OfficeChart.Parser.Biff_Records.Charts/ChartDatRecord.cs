using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartDat)]
[CLSCompliant(false)]
internal class ChartDatRecord : BiffRecordRaw
{
	private const int DefaultRecordSize = 2;

	[BiffRecordPos(0, 2)]
	private ushort m_usOptions;

	[BiffRecordPos(0, 0, TFieldType.Bit)]
	private bool m_bHasHorizontalBorders;

	[BiffRecordPos(0, 1, TFieldType.Bit)]
	private bool m_bHasVerticalBorders;

	[BiffRecordPos(0, 2, TFieldType.Bit)]
	private bool m_bHasBorders;

	[BiffRecordPos(0, 3, TFieldType.Bit)]
	private bool m_bShowSeriesKeys;

	public ushort Options => m_usOptions;

	public bool HasHorizontalBorders
	{
		get
		{
			return m_bHasHorizontalBorders;
		}
		set
		{
			m_bHasHorizontalBorders = value;
		}
	}

	public bool HasVerticalBorders
	{
		get
		{
			return m_bHasVerticalBorders;
		}
		set
		{
			m_bHasVerticalBorders = value;
		}
	}

	public bool HasBorders
	{
		get
		{
			return m_bHasBorders;
		}
		set
		{
			m_bHasBorders = value;
		}
	}

	public bool ShowSeriesKeys
	{
		get
		{
			return m_bShowSeriesKeys;
		}
		set
		{
			m_bShowSeriesKeys = value;
		}
	}

	public ChartDatRecord()
	{
	}

	public ChartDatRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartDatRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usOptions = provider.ReadUInt16(iOffset);
		m_bHasHorizontalBorders = provider.ReadBit(iOffset, 0);
		m_bHasVerticalBorders = provider.ReadBit(iOffset, 1);
		m_bHasBorders = provider.ReadBit(iOffset, 2);
		m_bShowSeriesKeys = provider.ReadBit(iOffset, 3);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_usOptions &= 15;
		provider.WriteUInt16(iOffset, m_usOptions);
		provider.WriteBit(iOffset, m_bHasHorizontalBorders, 0);
		provider.WriteBit(iOffset, m_bHasVerticalBorders, 1);
		provider.WriteBit(iOffset, m_bHasBorders, 2);
		provider.WriteBit(iOffset, m_bShowSeriesKeys, 3);
		m_iLength = 2;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 2;
	}
}
