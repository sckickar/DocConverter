using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartAxisParent)]
[CLSCompliant(false)]
internal class ChartAxisParentRecord : BiffRecordRaw
{
	private const int DEF_RECORD_SIZE = 18;

	[BiffRecordPos(0, 2)]
	private ushort m_usAxisIndex;

	[BiffRecordPos(2, 4, true)]
	private int m_iTopLeftX;

	[BiffRecordPos(6, 4, true)]
	private int m_iTopLeftY;

	[BiffRecordPos(10, 4, true)]
	private int m_iXLength;

	[BiffRecordPos(14, 4, true)]
	private int m_iYLength;

	public ushort AxesIndex
	{
		get
		{
			return m_usAxisIndex;
		}
		set
		{
			if (value != m_usAxisIndex)
			{
				m_usAxisIndex = value;
			}
		}
	}

	public int TopLeftX
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

	public int TopLeftY
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

	public int XAxisLength
	{
		get
		{
			return m_iXLength;
		}
		set
		{
			if (value != m_iXLength)
			{
				m_iXLength = value;
			}
		}
	}

	public int YAxisLength
	{
		get
		{
			return m_iYLength;
		}
		set
		{
			if (value != m_iYLength)
			{
				m_iYLength = value;
			}
		}
	}

	public ChartAxisParentRecord()
	{
	}

	public ChartAxisParentRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartAxisParentRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usAxisIndex = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_iTopLeftX = provider.ReadInt32(iOffset);
		iOffset += 4;
		m_iTopLeftY = provider.ReadInt32(iOffset);
		iOffset += 4;
		m_iXLength = provider.ReadInt32(iOffset);
		iOffset += 4;
		m_iYLength = provider.ReadInt32(iOffset);
		iOffset += 4;
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = GetStoreSize(version);
		provider.WriteUInt16(iOffset, m_usAxisIndex);
		iOffset += 2;
		provider.WriteInt32(iOffset, m_iTopLeftX);
		iOffset += 4;
		provider.WriteInt32(iOffset, m_iTopLeftY);
		iOffset += 4;
		provider.WriteInt32(iOffset, m_iXLength);
		iOffset += 4;
		provider.WriteInt32(iOffset, m_iYLength);
		iOffset += 4;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 18;
	}
}
