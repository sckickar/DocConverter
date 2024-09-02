using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartCatserRange)]
[CLSCompliant(false)]
internal class ChartCatserRangeRecord : BiffRecordRaw, IMaxCross
{
	private const int DEF_MIN_CROSSPOINT = 1;

	private const int DEF_MAX_CROSSPOINT = 31999;

	private const int DEF_RECORD_SIZE = 8;

	[BiffRecordPos(0, 2)]
	private ushort m_usCrossingPoint = 1;

	[BiffRecordPos(2, 2)]
	private ushort m_usLabelsFrequency = 1;

	[BiffRecordPos(4, 2)]
	private ushort m_usTickMarksFrequency = 1;

	[BiffRecordPos(6, 2)]
	private ushort m_usOptions = 1;

	[BiffRecordPos(6, 0, TFieldType.Bit)]
	private bool m_bValueAxisCrossing = true;

	[BiffRecordPos(6, 1, TFieldType.Bit)]
	private bool m_bMaxCross;

	[BiffRecordPos(6, 2, TFieldType.Bit)]
	private bool m_bReverse;

	public ushort CrossingPoint
	{
		get
		{
			return m_usCrossingPoint;
		}
		set
		{
			if (value != m_usCrossingPoint)
			{
				m_usCrossingPoint = value;
			}
		}
	}

	public ushort LabelsFrequency
	{
		get
		{
			return m_usLabelsFrequency;
		}
		set
		{
			if (value < 1 || value > 31999)
			{
				throw new ArgumentOutOfRangeException("value", "Value cannot be less 1 and greater than 31999");
			}
			if (value != m_usLabelsFrequency)
			{
				m_usLabelsFrequency = value;
			}
		}
	}

	public ushort TickMarksFrequency
	{
		get
		{
			return m_usTickMarksFrequency;
		}
		set
		{
			if (value < 1 || value > 31999)
			{
				throw new ArgumentOutOfRangeException("value", "Value cannot be less 1 and greater than 31999");
			}
			if (value != m_usTickMarksFrequency)
			{
				m_usTickMarksFrequency = value;
			}
		}
	}

	public ushort Options => m_usOptions;

	public bool IsBetween
	{
		get
		{
			return m_bValueAxisCrossing;
		}
		set
		{
			m_bValueAxisCrossing = value;
		}
	}

	public bool IsMaxCross
	{
		get
		{
			return m_bMaxCross;
		}
		set
		{
			m_bMaxCross = value;
		}
	}

	public bool IsReverse
	{
		get
		{
			return m_bReverse;
		}
		set
		{
			m_bReverse = value;
		}
	}

	public ChartCatserRangeRecord()
	{
	}

	public ChartCatserRangeRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartCatserRangeRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usCrossingPoint = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usLabelsFrequency = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usTickMarksFrequency = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usOptions = provider.ReadUInt16(iOffset);
		m_bValueAxisCrossing = provider.ReadBit(iOffset, 0);
		m_bMaxCross = provider.ReadBit(iOffset, 1);
		m_bReverse = provider.ReadBit(iOffset, 2);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = GetStoreSize(version);
		provider.WriteUInt16(iOffset, m_usCrossingPoint);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usLabelsFrequency);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usTickMarksFrequency);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usOptions);
		provider.WriteBit(iOffset, m_bValueAxisCrossing, 0);
		provider.WriteBit(iOffset, m_bMaxCross, 1);
		provider.WriteBit(iOffset, m_bReverse, 2);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 8;
	}
}
