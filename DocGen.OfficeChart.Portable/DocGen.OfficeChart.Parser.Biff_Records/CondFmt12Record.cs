using System;
using System.Collections.Generic;
using System.IO;
using DocGen.Drawing;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.CondFMT12)]
[CLSCompliant(false)]
internal class CondFmt12Record : BiffRecordRaw
{
	private const ushort DEF_MINIMUM_RECORD_SIZE = 26;

	private const int DEF_SUB_ITEM_SIZE = 8;

	private const ushort DEF_REDRAW_ON = 1;

	private const ushort DEF_REDRAW_OFF = 0;

	private FutureHeader m_header;

	private ushort m_attribute = 1;

	private TAddr m_addrEncloseRange;

	private ushort m_CF12Count;

	private bool m_NeedRedraw = true;

	private ushort m_index;

	private ushort m_usCellsCount;

	private List<Rectangle> m_arrCells = new List<Rectangle>();

	private bool m_isparsed;

	public ushort CF12RecordCount
	{
		get
		{
			return m_CF12Count;
		}
		set
		{
			m_CF12Count = value;
		}
	}

	public bool NeedRedrawRule
	{
		get
		{
			return m_NeedRedraw;
		}
		set
		{
			m_NeedRedraw = value;
		}
	}

	public ushort Index
	{
		get
		{
			return m_index;
		}
		set
		{
			m_index = value;
		}
	}

	public TAddr EncloseRange
	{
		get
		{
			return m_addrEncloseRange;
		}
		set
		{
			m_addrEncloseRange = value;
		}
	}

	public ushort CellsCount
	{
		get
		{
			return m_usCellsCount;
		}
		set
		{
			m_usCellsCount = value;
		}
	}

	public List<Rectangle> CellList
	{
		get
		{
			return m_arrCells;
		}
		internal set
		{
			m_arrCells = value;
		}
	}

	public bool IsParsed
	{
		get
		{
			return m_isparsed;
		}
		set
		{
			m_isparsed = value;
		}
	}

	public CondFmt12Record()
	{
		m_header = new FutureHeader();
		m_header.Type = 2169;
	}

	public CondFmt12Record(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public CondFmt12Record(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_isparsed = true;
		m_header.Type = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_attribute = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_addrEncloseRange = provider.ReadAddr(iOffset);
		iOffset += 8;
		m_CF12Count = provider.ReadUInt16(iOffset);
		iOffset += 2;
		ushort num = provider.ReadUInt16(iOffset);
		m_NeedRedraw = (num & 1) == 1;
		m_index = (ushort)(num >> 1);
		iOffset += 2;
		m_addrEncloseRange = provider.ReadAddr(iOffset);
		iOffset += 8;
		ExtractCellsList(provider, ref iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_header.Type);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_attribute);
		iOffset += 2;
		provider.WriteAddr(iOffset, m_addrEncloseRange);
		iOffset += 8;
		provider.WriteUInt16(iOffset, m_CF12Count);
		iOffset += 2;
		int num = (m_NeedRedraw ? ((m_index << 1) | 1) : ((m_index << 1) | 0));
		provider.WriteUInt16(iOffset, (ushort)num);
		iOffset += 2;
		provider.WriteAddr(iOffset, m_addrEncloseRange);
		iOffset += 8;
		provider.WriteUInt16(iOffset, m_usCellsCount);
		iOffset += 2;
		int num2 = 0;
		while (num2 < m_usCellsCount)
		{
			Rectangle addr = m_arrCells[num2];
			provider.WriteAddr(iOffset, addr);
			num2++;
			iOffset += 8;
		}
	}

	private void ExtractCellsList(DataProvider provider, ref int offset)
	{
		m_usCellsCount = provider.ReadUInt16(offset);
		offset += 2;
		int num = 0;
		while (num < m_usCellsCount)
		{
			Rectangle item = provider.ReadAddrAsRectangle(offset);
			m_arrCells.Add(item);
			num++;
			offset += 8;
		}
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 26 + m_arrCells.Count * 8;
	}

	public void AddCell(Rectangle addr)
	{
		m_arrCells.Add(addr);
		m_usCellsCount++;
	}

	public override object Clone()
	{
		CondFmt12Record condFmt12Record = (CondFmt12Record)base.Clone();
		condFmt12Record.m_arrCells = new List<Rectangle>(m_arrCells.Count);
		int i = 0;
		for (int count = m_arrCells.Count; i < count; i++)
		{
			condFmt12Record.AddCell(m_arrCells[i]);
		}
		return condFmt12Record;
	}
}
