using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartObjectLink)]
[CLSCompliant(false)]
internal class ChartObjectLinkRecord : BiffRecordRaw
{
	public const int DEF_RECORD_SIZE = 6;

	[BiffRecordPos(0, 2)]
	private ushort m_usLinkObject;

	[BiffRecordPos(2, 2)]
	private ushort m_usLinkIndex1;

	[BiffRecordPos(4, 2)]
	private ushort m_usLinkIndex2;

	public ExcelObjectTextLink LinkObject
	{
		get
		{
			return (ExcelObjectTextLink)m_usLinkObject;
		}
		set
		{
			m_usLinkObject = (ushort)value;
		}
	}

	public ushort SeriesNumber
	{
		get
		{
			return m_usLinkIndex1;
		}
		set
		{
			m_usLinkIndex1 = value;
		}
	}

	public ushort DataPointNumber
	{
		get
		{
			return m_usLinkIndex2;
		}
		set
		{
			m_usLinkIndex2 = value;
		}
	}

	public override int MinimumRecordSize => 6;

	public override int MaximumRecordSize => 6;

	public ChartObjectLinkRecord()
	{
	}

	public ChartObjectLinkRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartObjectLinkRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usLinkObject = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usLinkIndex1 = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usLinkIndex2 = provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = GetStoreSize(version);
		provider.WriteUInt16(iOffset, m_usLinkObject);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usLinkIndex1);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usLinkIndex2);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 6;
	}
}
