using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartPos)]
[CLSCompliant(false)]
internal class ChartPosRecord : BiffRecordRaw
{
	public const int DEF_RECORD_SIZE = 20;

	[BiffRecordPos(0, 2)]
	private ushort m_usTopLeft;

	[BiffRecordPos(2, 2)]
	private ushort m_usBottomRight = 2;

	[BiffRecordPos(4, 4, true)]
	private int m_iX1;

	[BiffRecordPos(8, 4, true)]
	private int m_iY1;

	[BiffRecordPos(12, 4, true)]
	private int m_iX2;

	[BiffRecordPos(16, 4, true)]
	private int m_iY2;

	public ushort TopLeft
	{
		get
		{
			return m_usTopLeft;
		}
		set
		{
			m_usTopLeft = value;
		}
	}

	public ushort BottomRight
	{
		get
		{
			return m_usBottomRight;
		}
		set
		{
			m_usBottomRight = value;
		}
	}

	public int X1
	{
		get
		{
			return m_iX1;
		}
		set
		{
			m_iX1 = value;
		}
	}

	public int Y1
	{
		get
		{
			return m_iY1;
		}
		set
		{
			m_iY1 = value;
		}
	}

	public int X2
	{
		get
		{
			return m_iX2;
		}
		set
		{
			m_iX2 = value;
		}
	}

	public int Y2
	{
		get
		{
			return m_iY2;
		}
		set
		{
			m_iY2 = value;
		}
	}

	public override int MinimumRecordSize => 20;

	public override int MaximumRecordSize => 20;

	public ChartPosRecord()
	{
	}

	public ChartPosRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartPosRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usTopLeft = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usBottomRight = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_iX1 = provider.ReadInt32(iOffset);
		iOffset += 4;
		m_iY1 = provider.ReadInt32(iOffset);
		iOffset += 4;
		m_iX2 = provider.ReadInt32(iOffset);
		iOffset += 4;
		m_iY2 = provider.ReadInt32(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = GetStoreSize(version);
		provider.WriteUInt16(iOffset, m_usTopLeft);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usBottomRight);
		iOffset += 2;
		provider.WriteInt32(iOffset, m_iX1);
		iOffset += 4;
		provider.WriteInt32(iOffset, m_iY1);
		iOffset += 4;
		provider.WriteInt32(iOffset, m_iX2);
		iOffset += 4;
		provider.WriteInt32(iOffset, m_iY2);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 20;
	}
}
