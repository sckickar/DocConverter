using System;
using System.Collections.Generic;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

[MsoDrawing(MsoRecords.msofbtClientAnchor)]
[CLSCompliant(false)]
internal class MsofbtClientAnchor : MsoBase
{
	private const uint DEF_CELL_MASK = 65535u;

	private const uint DEF_OFFSET_MASK = 4294901760u;

	private const int DEF_OFFSET_START_BIT = 16;

	private const int DEF_SHORT_LENGTH = 8;

	private const int DEF_RECORD_SIZE = 18;

	[BiffRecordPos(0, 2)]
	private ushort m_usOptions;

	[BiffRecordPos(0, 0, TFieldType.Bit)]
	private bool m_bNotMoveWithCell;

	[BiffRecordPos(0, 1, TFieldType.Bit)]
	private bool m_bNotSizeWithCell;

	[BiffRecordPos(2, 4)]
	private uint m_uiLeft;

	[BiffRecordPos(10, 4)]
	private uint m_uiRight;

	private bool m_bShortVersion;

	private int m_iTopRow;

	private int m_iTopOffset;

	private int m_iBottomRow;

	private int m_iBottomOffset;

	private bool m_bOneCellAnchor;

	public ushort Options
	{
		get
		{
			return m_usOptions;
		}
		set
		{
			m_usOptions = value;
		}
	}

	public bool IsSizeWithCell
	{
		get
		{
			return !m_bNotSizeWithCell;
		}
		set
		{
			m_bNotSizeWithCell = !value;
		}
	}

	public bool IsMoveWithCell
	{
		get
		{
			return !m_bNotMoveWithCell;
		}
		set
		{
			m_bNotMoveWithCell = !value;
		}
	}

	public int LeftColumn
	{
		get
		{
			return (int)BiffRecordRaw.GetUInt32BitsByMask(m_uiLeft, 65535u);
		}
		set
		{
			BiffRecordRaw.SetUInt32BitsByMask(ref m_uiLeft, 65535u, (uint)value);
		}
	}

	public int RightColumn
	{
		get
		{
			return (int)BiffRecordRaw.GetUInt32BitsByMask(m_uiRight, 65535u);
		}
		set
		{
			BiffRecordRaw.SetUInt32BitsByMask(ref m_uiRight, 65535u, (uint)value);
		}
	}

	public int TopRow
	{
		get
		{
			return m_iTopRow;
		}
		set
		{
			m_iTopRow = value;
		}
	}

	public int BottomRow
	{
		get
		{
			return m_iBottomRow;
		}
		set
		{
			m_iBottomRow = value;
		}
	}

	public int LeftOffset
	{
		get
		{
			return (int)(BiffRecordRaw.GetUInt32BitsByMask(m_uiLeft, 4294901760u) >> 16);
		}
		set
		{
			BiffRecordRaw.SetUInt32BitsByMask(ref m_uiLeft, 4294901760u, (uint)(value << 16));
		}
	}

	public int TopOffset
	{
		get
		{
			return m_iTopOffset;
		}
		set
		{
			m_iTopOffset = value;
		}
	}

	public int RightOffset
	{
		get
		{
			return (int)(BiffRecordRaw.GetUInt32BitsByMask(m_uiRight, 4294901760u) >> 16);
		}
		set
		{
			BiffRecordRaw.SetUInt32BitsByMask(ref m_uiRight, 4294901760u, (uint)(value << 16));
		}
	}

	public int BottomOffset
	{
		get
		{
			return m_iBottomOffset;
		}
		set
		{
			m_iBottomOffset = value;
		}
	}

	public bool IsShortVersion
	{
		get
		{
			return m_bShortVersion;
		}
		set
		{
			m_bShortVersion = value;
		}
	}

	public bool OneCellAnchor
	{
		get
		{
			return m_bOneCellAnchor;
		}
		set
		{
			m_bOneCellAnchor = value;
		}
	}

	public MsofbtClientAnchor(MsoBase parent)
		: base(parent)
	{
	}

	public MsofbtClientAnchor(MsoBase parent, byte[] data, int iOffset)
		: base(parent, data, iOffset)
	{
	}

	public override void InfillInternalData(Stream stream, int iOffset, List<int> arrBreaks, List<List<BiffRecordRaw>> arrRecords)
	{
		uint destination = 0u;
		uint destination2 = 0u;
		BiffRecordRaw.SetUInt32BitsByMask(ref destination, 65535u, (uint)m_iTopRow);
		BiffRecordRaw.SetUInt32BitsByMask(ref destination, 4294901760u, (uint)(m_iTopOffset << 16));
		BiffRecordRaw.SetUInt32BitsByMask(ref destination2, 65535u, (uint)m_iBottomRow);
		BiffRecordRaw.SetUInt32BitsByMask(ref destination2, 4294901760u, (uint)(m_iBottomOffset << 16));
		if (!m_bShortVersion)
		{
			m_iLength = 18;
			SetBitInVar(ref m_usOptions, m_bNotMoveWithCell, 0);
			SetBitInVar(ref m_usOptions, m_bNotSizeWithCell, 1);
			MsoBase.WriteUInt16(stream, m_usOptions);
			MsoBase.WriteUInt32(stream, m_uiLeft);
			MsoBase.WriteUInt32(stream, destination);
			MsoBase.WriteUInt32(stream, m_uiRight);
			MsoBase.WriteUInt32(stream, destination2);
		}
		else
		{
			m_iLength = 8;
			MsoBase.WriteUInt32(stream, m_uiLeft);
			MsoBase.WriteUInt32(stream, destination);
		}
	}

	public override void ParseStructure(Stream stream)
	{
		m_bShortVersion = m_iLength == 8;
		uint value = 0u;
		uint value2;
		if (!m_bShortVersion)
		{
			m_usOptions = MsoBase.ReadUInt16(stream);
			m_bNotMoveWithCell = BiffRecordRaw.GetBitFromVar(m_usOptions, 0);
			m_bNotSizeWithCell = BiffRecordRaw.GetBitFromVar(m_usOptions, 1);
			m_uiLeft = MsoBase.ReadUInt32(stream);
			value2 = MsoBase.ReadUInt32(stream);
			m_uiRight = MsoBase.ReadUInt32(stream);
			value = MsoBase.ReadUInt32(stream);
		}
		else
		{
			m_uiLeft = MsoBase.ReadUInt32(stream);
			value2 = MsoBase.ReadUInt32(stream);
		}
		m_iTopRow = (int)BiffRecordRaw.GetUInt32BitsByMask(value2, 65535u);
		m_iTopOffset = (int)(BiffRecordRaw.GetUInt32BitsByMask(value2, 4294901760u) >> 16);
		m_iBottomRow = (int)BiffRecordRaw.GetUInt32BitsByMask(value, 65535u);
		m_iBottomOffset = (int)(BiffRecordRaw.GetUInt32BitsByMask(value, 4294901760u) >> 16);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		if (!m_bShortVersion)
		{
			return 18;
		}
		return 8;
	}
}
