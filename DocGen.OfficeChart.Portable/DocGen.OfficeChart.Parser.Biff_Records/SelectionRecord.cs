using System;
using System.Collections.Generic;
using System.IO;
using DocGen.OfficeChart.Implementation.Exceptions;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.Selection)]
[CLSCompliant(false)]
internal class SelectionRecord : BiffRecordRaw, ICloneable
{
	public struct TAddr
	{
		public ushort m_usFirstRow;

		public ushort m_usLastRow;

		public byte m_FirstCol;

		public byte m_LastCol;

		public TAddr(ushort FirstRow, ushort LastRow, byte FirstCol, byte LastCol)
		{
			m_usFirstRow = FirstRow;
			m_usLastRow = LastRow;
			m_FirstCol = FirstCol;
			m_LastCol = LastCol;
		}

		public override string ToString()
		{
			return $"firstRow: {m_usFirstRow}, lastRow: {m_usLastRow}, firstColumn: {m_FirstCol}, lastColumn: {m_LastCol}";
		}
	}

	private const int DEF_FIXED_SIZE = 9;

	private const int DEF_SUB_ITEM_SIZE = 6;

	[BiffRecordPos(0, 1)]
	private byte m_Pane = 3;

	[BiffRecordPos(1, 2)]
	private ushort m_usRowActiveCell;

	[BiffRecordPos(3, 2)]
	private ushort m_usColActiveCell;

	[BiffRecordPos(5, 2)]
	private ushort m_usRefActiveCell;

	[BiffRecordPos(7, 2)]
	private ushort m_usNumRefs = 1;

	private List<TAddr> m_arrAddr = new List<TAddr>(new TAddr[1]);

	public byte Pane
	{
		get
		{
			return m_Pane;
		}
		set
		{
			m_Pane = value;
		}
	}

	public ushort RowActiveCell
	{
		get
		{
			return m_usRowActiveCell;
		}
		set
		{
			m_usRowActiveCell = value;
		}
	}

	public ushort ColumnActiveCell
	{
		get
		{
			return m_usColActiveCell;
		}
		set
		{
			m_usColActiveCell = value;
		}
	}

	public ushort RefActiveCell
	{
		get
		{
			return m_usRefActiveCell;
		}
		set
		{
			m_usRefActiveCell = value;
		}
	}

	public ushort NumRefs => m_usNumRefs;

	public override int MinimumRecordSize => 9;

	public TAddr[] Addr
	{
		get
		{
			return m_arrAddr.ToArray();
		}
		set
		{
			m_arrAddr = new List<TAddr>(value);
			m_usNumRefs = (ushort)m_arrAddr.Count;
		}
	}

	public void SetSelection(int iIndex, TAddr addr)
	{
		if (iIndex >= NumRefs || iIndex < 0)
		{
			throw new ArgumentOutOfRangeException("iIndex");
		}
		m_arrAddr[iIndex] = addr;
	}

	public SelectionRecord()
	{
	}

	public SelectionRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public SelectionRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_Pane = provider.ReadByte(iOffset);
		m_usRowActiveCell = provider.ReadUInt16(iOffset + 1);
		m_usColActiveCell = provider.ReadUInt16(iOffset + 3);
		m_usRefActiveCell = provider.ReadUInt16(iOffset + 5);
		m_usNumRefs = provider.ReadUInt16(iOffset + 7);
		if (m_iLength < 9 + m_usNumRefs * 6)
		{
			throw new WrongBiffRecordDataException("Data length does not fit to number of refernces.");
		}
		TAddr item = default(TAddr);
		int num = 9;
		m_arrAddr.Clear();
		int num2 = 0;
		while (num2 < m_usNumRefs)
		{
			item.m_usFirstRow = provider.ReadUInt16(iOffset + num);
			item.m_usLastRow = provider.ReadUInt16(iOffset + num + 2);
			item.m_FirstCol = provider.ReadByte(iOffset + num + 4);
			item.m_LastCol = provider.ReadByte(iOffset + num + 5);
			m_arrAddr.Add(item);
			num2++;
			num += 6;
		}
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = 9;
		provider.WriteByte(iOffset, m_Pane);
		provider.WriteUInt16(iOffset + 1, m_usRowActiveCell);
		provider.WriteUInt16(iOffset + 3, m_usColActiveCell);
		provider.WriteUInt16(iOffset + 5, m_usRefActiveCell);
		provider.WriteUInt16(iOffset + 7, m_usNumRefs);
		int num = 0;
		while (num < m_usNumRefs)
		{
			TAddr tAddr = m_arrAddr[num];
			provider.WriteUInt16(iOffset + m_iLength, tAddr.m_usFirstRow);
			provider.WriteUInt16(iOffset + m_iLength + 2, tAddr.m_usLastRow);
			provider.WriteByte(iOffset + m_iLength + 4, tAddr.m_FirstCol);
			provider.WriteByte(iOffset + m_iLength + 5, tAddr.m_LastCol);
			num++;
			m_iLength += 6;
		}
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 9 + m_arrAddr.Count * 6;
	}

	public new object Clone()
	{
		SelectionRecord obj = (SelectionRecord)MemberwiseClone();
		obj.m_arrAddr = new List<TAddr>(m_arrAddr);
		return obj;
	}
}
