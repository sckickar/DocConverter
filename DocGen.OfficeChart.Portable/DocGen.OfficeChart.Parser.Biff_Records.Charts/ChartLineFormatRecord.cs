using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartLineFormat)]
[CLSCompliant(false)]
internal class ChartLineFormatRecord : BiffRecordRaw
{
	public const int DEF_RECORD_SIZE = 12;

	[BiffRecordPos(0, 4, true)]
	private int m_rgbColor;

	[BiffRecordPos(4, 2)]
	private ushort m_usLinePattern;

	[BiffRecordPos(6, 2)]
	private ushort m_usLineWeight;

	[BiffRecordPos(8, 2)]
	private ushort m_usOptions;

	[BiffRecordPos(8, 0, TFieldType.Bit)]
	private bool m_bAutoFormat = true;

	[BiffRecordPos(8, 2, TFieldType.Bit)]
	private bool m_bDrawTickLabels;

	[BiffRecordPos(8, 3, TFieldType.Bit)]
	private bool m_bIsAutoLineColor = true;

	[BiffRecordPos(10, 2)]
	private ushort m_usColorIndex;

	public int LineColor
	{
		get
		{
			return m_rgbColor;
		}
		set
		{
			if (value != m_rgbColor)
			{
				m_rgbColor = value;
			}
		}
	}

	public OfficeChartLinePattern LinePattern
	{
		get
		{
			return (OfficeChartLinePattern)m_usLinePattern;
		}
		set
		{
			m_usLinePattern = (ushort)value;
		}
	}

	public OfficeChartLineWeight LineWeight
	{
		get
		{
			return (OfficeChartLineWeight)m_usLineWeight;
		}
		set
		{
			m_usLineWeight = (ushort)value;
		}
	}

	public ushort Options => m_usOptions;

	public bool AutoFormat
	{
		get
		{
			return m_bAutoFormat;
		}
		set
		{
			m_bAutoFormat = value;
		}
	}

	public bool DrawTickLabels
	{
		get
		{
			return m_bDrawTickLabels;
		}
		set
		{
			m_bDrawTickLabels = value;
		}
	}

	public bool IsAutoLineColor
	{
		get
		{
			return m_bIsAutoLineColor;
		}
		set
		{
			m_bIsAutoLineColor = value;
		}
	}

	public ushort ColorIndex
	{
		get
		{
			return m_usColorIndex;
		}
		set
		{
			m_usColorIndex = value;
		}
	}

	public override int MinimumRecordSize => 12;

	public override int MaximumRecordSize => 12;

	public ChartLineFormatRecord()
	{
	}

	public ChartLineFormatRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartLineFormatRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_rgbColor = provider.ReadInt32(iOffset);
		iOffset += 4;
		m_usLinePattern = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usLineWeight = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usOptions = provider.ReadUInt16(iOffset);
		m_bAutoFormat = provider.ReadBit(iOffset, 0);
		m_bDrawTickLabels = provider.ReadBit(iOffset, 2);
		m_bIsAutoLineColor = provider.ReadBit(iOffset, 3);
		iOffset += 2;
		m_usColorIndex = provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteInt32(iOffset, m_rgbColor);
		iOffset += 4;
		provider.WriteUInt16(iOffset, m_usLinePattern);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usLineWeight);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usOptions);
		provider.WriteBit(iOffset, m_bAutoFormat, 0);
		provider.WriteBit(iOffset, m_bDrawTickLabels, 2);
		provider.WriteBit(iOffset, m_bIsAutoLineColor, 3);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usColorIndex);
		m_iLength = GetStoreSize(version);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 12;
	}
}
