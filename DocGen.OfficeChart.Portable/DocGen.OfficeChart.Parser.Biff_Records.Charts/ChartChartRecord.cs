using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartChart)]
[CLSCompliant(false)]
internal class ChartChartRecord : BiffRecordRaw
{
	public const int DEF_RECORD_SIZE = 16;

	[BiffRecordPos(0, 4, true)]
	private int m_iTopLeftX;

	[BiffRecordPos(4, 4, true)]
	private int m_iTopLeftY;

	[BiffRecordPos(8, 4, true)]
	private int m_iWidth;

	[BiffRecordPos(12, 4, true)]
	private int m_iHeight;

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

	public override int MinimumRecordSize => 16;

	public override int MaximumRecordSize => 16;

	public ChartChartRecord()
	{
	}

	public ChartChartRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartChartRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_iTopLeftX = provider.ReadInt32(iOffset);
		iOffset += 4;
		m_iTopLeftY = provider.ReadInt32(iOffset);
		iOffset += 4;
		m_iWidth = provider.ReadInt32(iOffset);
		iOffset += 4;
		m_iHeight = provider.ReadInt32(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = GetStoreSize(version);
		provider.WriteInt32(iOffset, m_iTopLeftX);
		iOffset += 4;
		provider.WriteInt32(iOffset, m_iTopLeftY);
		iOffset += 4;
		provider.WriteInt32(iOffset, m_iWidth);
		iOffset += 4;
		provider.WriteInt32(iOffset, m_iHeight);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 16;
	}
}
