using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartLine)]
[CLSCompliant(false)]
internal class ChartLineRecord : BiffRecordRaw, IChartType
{
	private const int DefaultRecordSize = 2;

	[BiffRecordPos(0, 2)]
	private ushort m_usOptions;

	[BiffRecordPos(0, 0, TFieldType.Bit)]
	private bool m_bStackValues;

	[BiffRecordPos(0, 1, TFieldType.Bit)]
	private bool m_bAsPercents;

	[BiffRecordPos(0, 2, TFieldType.Bit)]
	private bool m_bHasShadow;

	private ushort m_usGap;

	public ushort Options => m_usOptions;

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

	internal ushort Gapwidth
	{
		get
		{
			return m_usGap;
		}
		set
		{
			if (value != m_usGap)
			{
				m_usGap = value;
			}
		}
	}

	public ChartLineRecord()
	{
	}

	public ChartLineRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartLineRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usOptions = provider.ReadUInt16(iOffset);
		m_bStackValues = provider.ReadBit(iOffset, 0);
		m_bAsPercents = provider.ReadBit(iOffset, 1);
		m_bHasShadow = provider.ReadBit(iOffset, 2);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_usOptions &= 7;
		provider.WriteUInt16(iOffset, m_usOptions);
		provider.WriteBit(iOffset, m_bStackValues, 0);
		provider.WriteBit(iOffset, m_bAsPercents, 1);
		provider.WriteBit(iOffset, m_bHasShadow, 2);
		m_iLength = 2;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 2;
	}
}
