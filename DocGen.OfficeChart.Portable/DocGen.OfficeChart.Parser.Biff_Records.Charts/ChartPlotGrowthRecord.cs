using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartPlotGrowth)]
[CLSCompliant(false)]
internal class ChartPlotGrowthRecord : BiffRecordRaw
{
	public const int DEF_RECORD_SIZE = 8;

	[BiffRecordPos(0, 4)]
	private uint m_uiHorzGrowth = 65536u;

	[BiffRecordPos(4, 4)]
	private uint m_uiVertGrowth = 65536u;

	public uint HorzGrowth
	{
		get
		{
			return m_uiHorzGrowth;
		}
		set
		{
			m_uiHorzGrowth = value;
		}
	}

	public uint VertGrowth
	{
		get
		{
			return m_uiVertGrowth;
		}
		set
		{
			m_uiVertGrowth = value;
		}
	}

	public override int MinimumRecordSize => 8;

	public override int MaximumRecordSize => 8;

	public ChartPlotGrowthRecord()
	{
	}

	public ChartPlotGrowthRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartPlotGrowthRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_uiHorzGrowth = provider.ReadUInt32(iOffset);
		iOffset += 4;
		m_uiVertGrowth = provider.ReadUInt32(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = GetStoreSize(version);
		provider.WriteUInt32(iOffset, m_uiHorzGrowth);
		iOffset += 4;
		provider.WriteUInt32(iOffset, m_uiVertGrowth);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 8;
	}
}
