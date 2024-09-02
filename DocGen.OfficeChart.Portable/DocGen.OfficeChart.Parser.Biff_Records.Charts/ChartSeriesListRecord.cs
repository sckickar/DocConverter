using System;
using System.IO;
using DocGen.OfficeChart.Implementation.Exceptions;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartSeriesList)]
[CLSCompliant(false)]
internal class ChartSeriesListRecord : BiffRecordRaw
{
	public const int DEF_RECORD_SIZE = 2;

	[BiffRecordPos(0, 2)]
	private ushort m_usCount;

	private ushort[] m_arrSeries;

	public ushort SeriesCount => m_usCount;

	public ushort[] Series
	{
		get
		{
			return m_arrSeries;
		}
		set
		{
			m_arrSeries = value;
			m_usCount = (ushort)((value != null) ? ((uint)value.Length) : 0u);
		}
	}

	public override int MinimumRecordSize => 2;

	public ChartSeriesListRecord()
	{
	}

	public ChartSeriesListRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartSeriesListRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usCount = provider.ReadUInt16(iOffset);
		iOffset += 2;
		if (m_usCount * 2 + 2 != m_iLength)
		{
			throw new WrongBiffRecordDataException("ChartListRecord");
		}
		m_arrSeries = new ushort[m_usCount];
		int num = 0;
		while (num < m_usCount)
		{
			m_arrSeries[num] = provider.ReadUInt16(iOffset);
			num++;
			iOffset += 2;
		}
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usCount);
		m_iLength = 2;
		int num = 0;
		while (num < m_usCount)
		{
			provider.WriteUInt16(iOffset + m_iLength, m_arrSeries[num]);
			num++;
			m_iLength += 2;
		}
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 2 + 2 * m_usCount;
	}

	public static bool operator ==(ChartSeriesListRecord record1, ChartSeriesListRecord record2)
	{
		bool flag = object.Equals(record1, null);
		bool flag2 = object.Equals(record2, null);
		if (flag && flag2)
		{
			return true;
		}
		if (flag || flag2)
		{
			return false;
		}
		bool flag3 = record1.m_usCount == record2.m_usCount;
		int i = 0;
		for (int usCount = record1.m_usCount; i < usCount && flag3; i++)
		{
			flag3 = record1.m_arrSeries[i] == record2.m_arrSeries[i];
		}
		return flag3;
	}

	public static bool operator !=(ChartSeriesListRecord record1, ChartSeriesListRecord record2)
	{
		return !(record1 == record2);
	}

	public override object Clone()
	{
		ChartSeriesListRecord obj = (ChartSeriesListRecord)base.Clone();
		obj.m_arrSeries = CloneUtils.CloneUshortArray(m_arrSeries);
		return obj;
	}
}
