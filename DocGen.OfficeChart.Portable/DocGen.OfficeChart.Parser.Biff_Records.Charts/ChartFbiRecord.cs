using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartFbi)]
[CLSCompliant(false)]
internal class ChartFbiRecord : BiffRecordRaw
{
	private const int DEF_RECORD_SIZE = 10;

	[BiffRecordPos(0, 2)]
	private ushort m_usBasisWidth;

	[BiffRecordPos(2, 2)]
	private ushort m_usBasisHeight;

	[BiffRecordPos(4, 2)]
	private ushort m_usAppliedFontHeight;

	[BiffRecordPos(6, 2)]
	private ushort m_usScaleBasis;

	[BiffRecordPos(8, 2)]
	private ushort m_usFontIndex;

	public ushort BasisWidth
	{
		get
		{
			return m_usBasisWidth;
		}
		set
		{
			if (value != m_usBasisWidth)
			{
				m_usBasisWidth = value;
			}
		}
	}

	public ushort BasisHeight
	{
		get
		{
			return m_usBasisHeight;
		}
		set
		{
			if (value != m_usBasisHeight)
			{
				m_usBasisHeight = value;
			}
		}
	}

	public ushort AppliedFontHeight
	{
		get
		{
			return m_usAppliedFontHeight;
		}
		set
		{
			if (value != m_usAppliedFontHeight)
			{
				m_usAppliedFontHeight = value;
			}
		}
	}

	public ushort ScaleBasis
	{
		get
		{
			return m_usScaleBasis;
		}
		set
		{
			if (value != m_usScaleBasis)
			{
				m_usScaleBasis = value;
			}
		}
	}

	public ushort FontIndex
	{
		get
		{
			return m_usFontIndex;
		}
		set
		{
			if (value != m_usFontIndex)
			{
				m_usFontIndex = value;
			}
		}
	}

	public ChartFbiRecord()
	{
	}

	public ChartFbiRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartFbiRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usBasisWidth = provider.ReadUInt16(iOffset);
		m_usBasisHeight = provider.ReadUInt16(iOffset + 2);
		m_usAppliedFontHeight = provider.ReadUInt16(iOffset + 4);
		m_usScaleBasis = provider.ReadUInt16(iOffset + 6);
		m_usFontIndex = provider.ReadUInt16(iOffset + 8);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = GetStoreSize(version);
		provider.WriteUInt16(iOffset, m_usBasisWidth);
		provider.WriteUInt16(iOffset + 2, m_usBasisHeight);
		provider.WriteUInt16(iOffset + 4, m_usAppliedFontHeight);
		provider.WriteUInt16(iOffset + 6, m_usScaleBasis);
		provider.WriteUInt16(iOffset + 8, m_usFontIndex);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 10;
	}
}
