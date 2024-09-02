using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartLegend)]
[CLSCompliant(false)]
internal class ChartLegendRecord : BiffRecordRaw
{
	private const int DefaultRecordSize = 20;

	[BiffRecordPos(0, 4, true)]
	private int m_iTopLeftX;

	[BiffRecordPos(4, 4, true)]
	private int m_iTopLeftY;

	[BiffRecordPos(8, 4, true)]
	private int m_iWidth;

	[BiffRecordPos(12, 4, true)]
	private int m_iHeight;

	[BiffRecordPos(16, 1)]
	private byte m_wType = 3;

	[BiffRecordPos(17, 1)]
	private byte m_wSpacing = 1;

	[BiffRecordPos(18, 2)]
	private ushort m_usOptions;

	[BiffRecordPos(18, 0, TFieldType.Bit)]
	private bool m_bAutoPosition = true;

	[BiffRecordPos(18, 1, TFieldType.Bit)]
	private bool m_bAutoSeries = true;

	[BiffRecordPos(18, 2, TFieldType.Bit)]
	private bool m_bAutoPosX = true;

	[BiffRecordPos(18, 3, TFieldType.Bit)]
	private bool m_bAutoPosY = true;

	[BiffRecordPos(18, 4, TFieldType.Bit)]
	private bool m_bIsVerticalLegend = true;

	[BiffRecordPos(18, 5, TFieldType.Bit)]
	private bool m_bContainsDataTable;

	public int X
	{
		get
		{
			return m_iTopLeftX;
		}
		set
		{
			if (value != m_iTopLeftX)
			{
				m_iTopLeftX = value;
			}
		}
	}

	public int Y
	{
		get
		{
			return m_iTopLeftY;
		}
		set
		{
			if (value != m_iTopLeftY)
			{
				m_iTopLeftY = value;
			}
		}
	}

	public int Width
	{
		get
		{
			return m_iWidth;
		}
		set
		{
			if (value != m_iWidth)
			{
				m_iWidth = value;
			}
		}
	}

	public int Height
	{
		get
		{
			return m_iHeight;
		}
		set
		{
			if (value != m_iHeight)
			{
				m_iHeight = value;
			}
		}
	}

	public OfficeLegendPosition Position
	{
		get
		{
			return (OfficeLegendPosition)m_wType;
		}
		set
		{
			m_wType = (byte)value;
		}
	}

	public ExcelLegendSpacing Spacing
	{
		get
		{
			return (ExcelLegendSpacing)m_wSpacing;
		}
		set
		{
			m_wSpacing = (byte)value;
		}
	}

	public bool AutoPosition
	{
		get
		{
			return m_bAutoPosition;
		}
		set
		{
			m_bAutoPosition = value;
		}
	}

	public bool AutoSeries
	{
		get
		{
			return m_bAutoSeries;
		}
		set
		{
			m_bAutoSeries = value;
		}
	}

	public bool AutoPositionX
	{
		get
		{
			return m_bAutoPosX;
		}
		set
		{
			m_bAutoPosX = value;
		}
	}

	public bool AutoPositionY
	{
		get
		{
			return m_bAutoPosY;
		}
		set
		{
			m_bAutoPosY = value;
		}
	}

	public bool IsVerticalLegend
	{
		get
		{
			return m_bIsVerticalLegend;
		}
		set
		{
			m_bIsVerticalLegend = value;
		}
	}

	public bool ContainsDataTable
	{
		get
		{
			return m_bContainsDataTable;
		}
		set
		{
			m_bContainsDataTable = value;
		}
	}

	public ChartLegendRecord()
	{
	}

	public ChartLegendRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartLegendRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_iTopLeftX = provider.ReadInt32(iOffset);
		m_iTopLeftY = provider.ReadInt32(iOffset + 4);
		m_iWidth = provider.ReadInt32(iOffset + 8);
		m_iHeight = provider.ReadInt32(iOffset + 12);
		m_wType = provider.ReadByte(iOffset + 16);
		m_wSpacing = provider.ReadByte(iOffset + 17);
		m_usOptions = provider.ReadUInt16(iOffset + 18);
		m_bAutoPosition = provider.ReadBit(iOffset + 18, 0);
		m_bAutoSeries = provider.ReadBit(iOffset + 18, 1);
		m_bAutoPosX = provider.ReadBit(iOffset + 18, 2);
		m_bAutoPosY = provider.ReadBit(iOffset + 18, 3);
		m_bIsVerticalLegend = provider.ReadBit(iOffset + 18, 4);
		m_bContainsDataTable = provider.ReadBit(iOffset + 18, 5);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_usOptions &= 63;
		provider.WriteInt32(iOffset, m_iTopLeftX);
		provider.WriteInt32(iOffset + 4, m_iTopLeftY);
		provider.WriteInt32(iOffset + 8, m_iWidth);
		provider.WriteInt32(iOffset + 12, m_iHeight);
		provider.WriteByte(iOffset + 16, m_wType);
		provider.WriteByte(iOffset + 17, m_wSpacing);
		provider.WriteUInt16(iOffset + 18, m_usOptions);
		provider.WriteBit(iOffset + 18, m_bAutoPosition, 0);
		provider.WriteBit(iOffset + 18, m_bAutoSeries, 1);
		provider.WriteBit(iOffset + 18, m_bAutoPosX, 2);
		provider.WriteBit(iOffset + 18, m_bAutoPosY, 3);
		provider.WriteBit(iOffset + 18, m_bIsVerticalLegend, 4);
		provider.WriteBit(iOffset + 18, m_bContainsDataTable, 5);
		m_iLength = 20;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 20;
	}
}
