using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartFrame)]
[CLSCompliant(false)]
internal class ChartFrameRecord : BiffRecordRaw
{
	public const int DEF_RECORD_SIZE = 4;

	[BiffRecordPos(0, 2)]
	private ushort m_usRectStyle;

	[BiffRecordPos(2, 2)]
	private ushort m_usOptions;

	[BiffRecordPos(2, 0, TFieldType.Bit)]
	private bool m_bAutoSize = true;

	[BiffRecordPos(2, 1, TFieldType.Bit)]
	private bool m_bAutoPosition = true;

	public ushort Options => m_usOptions;

	public OfficeRectangleStyle Rectangle
	{
		get
		{
			return (OfficeRectangleStyle)m_usRectStyle;
		}
		set
		{
			m_usRectStyle = (ushort)value;
		}
	}

	public bool AutoSize
	{
		get
		{
			return m_bAutoSize;
		}
		set
		{
			m_bAutoSize = value;
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

	public override int MinimumRecordSize => 4;

	public override int MaximumRecordSize => 4;

	public ChartFrameRecord()
	{
	}

	public ChartFrameRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartFrameRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usRectStyle = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usOptions = provider.ReadUInt16(iOffset);
		m_bAutoSize = provider.ReadBit(iOffset, 0);
		m_bAutoPosition = provider.ReadBit(iOffset, 1);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = GetStoreSize(version);
		provider.WriteUInt16(iOffset, m_usRectStyle);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usOptions);
		provider.WriteBit(iOffset, m_bAutoSize, 0);
		provider.WriteBit(iOffset, m_bAutoPosition, 1);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 4;
	}
}
