using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartSurface)]
[CLSCompliant(false)]
internal class ChartSurfaceRecord : BiffRecordRaw
{
	public const int DEF_RECORD_SIZE = 2;

	[BiffRecordPos(0, 2)]
	private ushort m_usOptions;

	[BiffRecordPos(0, 0, TFieldType.Bit)]
	private bool m_bFillSurface;

	[BiffRecordPos(0, 1, TFieldType.Bit)]
	private bool m_b3DPhongShade;

	public ushort Options => m_usOptions;

	public bool IsFillSurface
	{
		get
		{
			return m_bFillSurface;
		}
		set
		{
			m_bFillSurface = value;
		}
	}

	public bool Is3DPhongShade
	{
		get
		{
			return m_b3DPhongShade;
		}
		set
		{
			m_b3DPhongShade = value;
		}
	}

	public override int MinimumRecordSize => 2;

	public override int MaximumRecordSize => 2;

	public ChartSurfaceRecord()
	{
	}

	public ChartSurfaceRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartSurfaceRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usOptions = provider.ReadUInt16(iOffset);
		m_bFillSurface = provider.ReadBit(iOffset, 0);
		m_b3DPhongShade = provider.ReadBit(iOffset, 1);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usOptions);
		provider.WriteBit(iOffset, m_bFillSurface, 0);
		provider.WriteBit(iOffset, m_b3DPhongShade, 1);
		m_iLength = 2;
	}
}
