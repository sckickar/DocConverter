using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartAxis)]
[CLSCompliant(false)]
internal class ChartAxisRecord : BiffRecordRaw
{
	public enum ChartAxisType
	{
		CategoryAxis,
		ValueAxis,
		SeriesAxis
	}

	private const int DEF_RECORD_SIZE = 18;

	[BiffRecordPos(0, 2)]
	private ushort m_usAxisType;

	[BiffRecordPos(2, 4, true)]
	private int m_Reserved0;

	[BiffRecordPos(6, 4, true)]
	private int m_Reserved1;

	[BiffRecordPos(10, 4, true)]
	private int m_Reserved2;

	[BiffRecordPos(14, 4, true)]
	private int m_Reserved3;

	public ChartAxisType AxisType
	{
		get
		{
			return (ChartAxisType)m_usAxisType;
		}
		set
		{
			m_usAxisType = (ushort)value;
		}
	}

	public int Reserved0 => m_Reserved0;

	public int Reserved1 => m_Reserved1;

	public int Reserved2 => m_Reserved2;

	public int Reserved3 => m_Reserved3;

	public ChartAxisRecord()
	{
	}

	public ChartAxisRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartAxisRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usAxisType = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_Reserved0 = provider.ReadInt32(iOffset);
		iOffset += 4;
		m_Reserved1 = provider.ReadInt32(iOffset);
		iOffset += 4;
		m_Reserved2 = provider.ReadInt32(iOffset);
		iOffset += 4;
		m_Reserved3 = provider.ReadInt32(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_Reserved0 = (m_Reserved1 = (m_Reserved2 = (m_Reserved3 = 0)));
		m_iLength = GetStoreSize(version);
		provider.WriteUInt16(iOffset, m_usAxisType);
		iOffset += 2;
		provider.WriteInt32(iOffset, m_Reserved0);
		iOffset += 4;
		provider.WriteInt32(iOffset, m_Reserved1);
		iOffset += 4;
		provider.WriteInt32(iOffset, m_Reserved2);
		iOffset += 4;
		provider.WriteInt32(iOffset, m_Reserved3);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 18;
	}
}
