using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartArea)]
[CLSCompliant(false)]
internal class ChartAreaRecord : BiffRecordRaw, IChartType
{
	private const int DefaultRecordSize = 2;

	[BiffRecordPos(0, 2)]
	private ushort m_usOptions;

	[BiffRecordPos(0, 0, TFieldType.Bit)]
	private bool m_bStacked;

	[BiffRecordPos(0, 1, TFieldType.Bit)]
	private bool m_bCategoryPercentage;

	[BiffRecordPos(0, 2, TFieldType.Bit)]
	private bool m_bShadowArea;

	public ushort Options => m_usOptions;

	public bool IsStacked
	{
		get
		{
			return m_bStacked;
		}
		set
		{
			m_bStacked = value;
		}
	}

	public bool IsCategoryBrokenDown
	{
		get
		{
			return m_bCategoryPercentage;
		}
		set
		{
			m_bCategoryPercentage = value;
		}
	}

	public bool IsAreaShadowed
	{
		get
		{
			return m_bShadowArea;
		}
		set
		{
			m_bShadowArea = value;
		}
	}

	bool IChartType.ShowAsPercents
	{
		get
		{
			return IsCategoryBrokenDown;
		}
		set
		{
			IsCategoryBrokenDown = value;
		}
	}

	bool IChartType.StackValues
	{
		get
		{
			return IsStacked;
		}
		set
		{
			IsStacked = value;
		}
	}

	public ChartAreaRecord()
	{
	}

	public ChartAreaRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartAreaRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usOptions = provider.ReadUInt16(iOffset);
		m_bStacked = provider.ReadBit(iOffset, 0);
		m_bCategoryPercentage = provider.ReadBit(iOffset, 1);
		m_bShadowArea = provider.ReadBit(iOffset, 2);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_usOptions &= 7;
		provider.WriteUInt16(iOffset, m_usOptions);
		provider.WriteBit(iOffset, m_bStacked, 0);
		provider.WriteBit(iOffset, m_bCategoryPercentage, 1);
		provider.WriteBit(iOffset, m_bShadowArea, 2);
		m_iLength = 2;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 2;
	}
}
