using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.Table)]
[CLSCompliant(false)]
internal class TableRecord : BiffRecordRaw
{
	public const ushort OperationModeBitMask = 12;

	public const int OperationModeStartBit = 2;

	[BiffRecordPos(0, 2)]
	private ushort m_usFirstRow;

	[BiffRecordPos(2, 2)]
	private ushort m_usLastRow;

	[BiffRecordPos(4, 1)]
	private byte m_FirstCol;

	[BiffRecordPos(5, 1)]
	private byte m_LastCol;

	[BiffRecordPos(6, 2)]
	private ushort m_usOptions;

	[BiffRecordPos(6, 0, TFieldType.Bit)]
	private bool m_bRecalculate;

	[BiffRecordPos(6, 1, TFieldType.Bit)]
	private bool m_bCalculateOnOpen;

	[BiffRecordPos(8, 2)]
	private ushort m_usInputCellRow;

	[BiffRecordPos(10, 2)]
	private ushort m_usInputCellCol;

	[BiffRecordPos(12, 2)]
	private ushort m_usInputCellRowForCol;

	[BiffRecordPos(14, 2)]
	private ushort m_usInputCellColForCol;

	public ushort FirstRow
	{
		get
		{
			return m_usFirstRow;
		}
		set
		{
			m_usFirstRow = value;
		}
	}

	public ushort LastRow
	{
		get
		{
			return m_usLastRow;
		}
		set
		{
			m_usLastRow = value;
		}
	}

	public byte FirstCol
	{
		get
		{
			return m_FirstCol;
		}
		set
		{
			m_FirstCol = value;
		}
	}

	public byte LastCol
	{
		get
		{
			return m_LastCol;
		}
		set
		{
			m_LastCol = value;
		}
	}

	public bool IsRecalculate
	{
		get
		{
			return m_bRecalculate;
		}
		set
		{
			m_bRecalculate = value;
		}
	}

	public bool IsCalculateOnOpen
	{
		get
		{
			return m_bCalculateOnOpen;
		}
		set
		{
			m_bCalculateOnOpen = value;
		}
	}

	public ushort OperationMode
	{
		get
		{
			return (ushort)(BiffRecordRaw.GetUInt16BitsByMask(m_usOptions, 12) >> 2);
		}
		set
		{
			if (value > 4)
			{
				throw new ArgumentOutOfRangeException();
			}
			BiffRecordRaw.SetUInt16BitsByMask(ref m_usOptions, 12, (ushort)(value << 2));
		}
	}

	public ushort InputCellRow
	{
		get
		{
			return m_usInputCellRow;
		}
		set
		{
			m_usInputCellRow = value;
		}
	}

	public ushort InputCellColumn
	{
		get
		{
			return m_usInputCellCol;
		}
		set
		{
			m_usInputCellCol = value;
		}
	}

	public ushort InputCellRowForColumn
	{
		get
		{
			return m_usInputCellRowForCol;
		}
		set
		{
			m_usInputCellRowForCol = value;
		}
	}

	public ushort InputCellColumnForColumn
	{
		get
		{
			return m_usInputCellColForCol;
		}
		set
		{
			m_usInputCellColForCol = value;
		}
	}

	public override int MinimumRecordSize => 16;

	public override int MaximumRecordSize => 16;

	public TableRecord()
	{
	}

	public TableRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public TableRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usFirstRow = provider.ReadUInt16(iOffset);
		m_usLastRow = provider.ReadUInt16(iOffset + 2);
		m_FirstCol = provider.ReadByte(iOffset + 4);
		m_LastCol = provider.ReadByte(iOffset + 5);
		m_usOptions = provider.ReadUInt16(iOffset + 6);
		m_bRecalculate = provider.ReadBit(iOffset + 6, 0);
		m_bCalculateOnOpen = provider.ReadBit(iOffset + 6, 1);
		m_usInputCellRow = provider.ReadUInt16(iOffset + 8);
		m_usInputCellCol = provider.ReadUInt16(iOffset + 10);
		m_usInputCellRowForCol = provider.ReadUInt16(iOffset + 12);
		m_usInputCellColForCol = provider.ReadUInt16(iOffset + 14);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usFirstRow);
		provider.WriteUInt16(iOffset + 2, m_usLastRow);
		provider.WriteByte(iOffset + 4, m_FirstCol);
		provider.WriteByte(iOffset + 5, m_LastCol);
		provider.WriteUInt16(iOffset + 6, m_usOptions);
		provider.WriteBit(iOffset + 6, m_bRecalculate, 0);
		provider.WriteBit(iOffset + 6, m_bCalculateOnOpen, 1);
		provider.WriteUInt16(iOffset + 8, m_usInputCellRow);
		provider.WriteUInt16(iOffset + 10, m_usInputCellCol);
		provider.WriteUInt16(iOffset + 12, m_usInputCellRowForCol);
		provider.WriteUInt16(iOffset + 14, m_usInputCellColForCol);
		m_iLength = MinimumRecordSize;
	}
}
