using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartSerFmt)]
[CLSCompliant(false)]
internal class ChartSerFmtRecord : BiffRecordRaw
{
	public const int DefaultRecordSize = 2;

	[BiffRecordPos(0, 2)]
	private ushort m_usOptions;

	[BiffRecordPos(0, 0, TFieldType.Bit)]
	private bool m_bSmoothedLine;

	[BiffRecordPos(0, 1, TFieldType.Bit)]
	private bool m_b3DBubbles;

	[BiffRecordPos(0, 2, TFieldType.Bit)]
	private bool m_bArShadow;

	public ushort Options => m_usOptions;

	public bool IsSmoothedLine
	{
		get
		{
			return m_bSmoothedLine;
		}
		set
		{
			m_bSmoothedLine = value;
		}
	}

	public bool Is3DBubbles
	{
		get
		{
			return m_b3DBubbles;
		}
		set
		{
			m_b3DBubbles = value;
		}
	}

	public bool IsArShadow
	{
		get
		{
			return m_bArShadow;
		}
		set
		{
			m_bArShadow = value;
		}
	}

	public override int MinimumRecordSize => 2;

	public override int MaximumRecordSize => 2;

	public ChartSerFmtRecord()
	{
	}

	public ChartSerFmtRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartSerFmtRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usOptions = provider.ReadUInt16(iOffset);
		m_bSmoothedLine = provider.ReadBit(iOffset, 0);
		m_b3DBubbles = provider.ReadBit(iOffset, 1);
		m_bArShadow = provider.ReadBit(iOffset, 2);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usOptions);
		provider.WriteBit(iOffset, m_bSmoothedLine, 0);
		provider.WriteBit(iOffset, m_b3DBubbles, 1);
		provider.WriteBit(iOffset, m_bArShadow, 2);
		m_iLength = 2;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 2;
	}
}
