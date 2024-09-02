using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartBar)]
[CLSCompliant(false)]
internal class ChartBarRecord : BiffRecordRaw, IChartType
{
	private const int DEF_RECORD_SIZE = 6;

	[BiffRecordPos(0, 2)]
	private ushort m_usOverlap;

	[BiffRecordPos(2, 2)]
	private ushort m_usCategoriesSpace = 150;

	[BiffRecordPos(4, 2)]
	private ushort m_usOptions;

	[BiffRecordPos(4, 0, TFieldType.Bit)]
	private bool m_bIsHorizontal;

	[BiffRecordPos(4, 1, TFieldType.Bit)]
	private bool m_bStackValues;

	[BiffRecordPos(4, 2, TFieldType.Bit)]
	private bool m_bAsPercents;

	[BiffRecordPos(4, 3, TFieldType.Bit)]
	private bool m_bHasShadow;

	public int Overlap
	{
		get
		{
			return -(short)m_usOverlap;
		}
		set
		{
			if (value != Overlap)
			{
				m_usOverlap = (ushort)(-value);
			}
		}
	}

	public ushort CategoriesSpace
	{
		get
		{
			return m_usCategoriesSpace;
		}
		set
		{
			if (value != m_usCategoriesSpace)
			{
				m_usCategoriesSpace = value;
			}
		}
	}

	public ushort Options => m_usOptions;

	public bool IsHorizontalBar
	{
		get
		{
			return m_bIsHorizontal;
		}
		set
		{
			m_bIsHorizontal = value;
		}
	}

	public bool StackValues
	{
		get
		{
			return m_bStackValues;
		}
		set
		{
			m_bStackValues = value;
		}
	}

	public bool ShowAsPercents
	{
		get
		{
			return m_bAsPercents;
		}
		set
		{
			m_bAsPercents = value;
		}
	}

	public bool HasShadow
	{
		get
		{
			return m_bHasShadow;
		}
		set
		{
			m_bHasShadow = value;
		}
	}

	public ChartBarRecord()
	{
	}

	public ChartBarRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartBarRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usOverlap = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usCategoriesSpace = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usOptions = provider.ReadUInt16(iOffset);
		m_bIsHorizontal = provider.ReadBit(iOffset, 0);
		m_bStackValues = provider.ReadBit(iOffset, 1);
		m_bAsPercents = provider.ReadBit(iOffset, 2);
		m_bHasShadow = provider.ReadBit(iOffset, 3);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_usOptions &= 15;
		m_iLength = GetStoreSize(version);
		provider.WriteUInt16(iOffset, m_usOverlap);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usCategoriesSpace);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usOptions);
		provider.WriteBit(iOffset, m_bIsHorizontal, 0);
		provider.WriteBit(iOffset, m_bStackValues, 1);
		provider.WriteBit(iOffset, m_bAsPercents, 2);
		provider.WriteBit(iOffset, m_bHasShadow, 3);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 6;
	}
}
